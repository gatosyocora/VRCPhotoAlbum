using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Gatosyocora.VRCPhotoAlbum.Models.Entities;
using KoyashiroKohaku.VrcMetaTool;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using User = Gatosyocora.VRCPhotoAlbum.Models.Entities.User;

namespace Gatosyocora.VRCPhotoAlbum.Servisies
{
    public class DBCacheService
    {
        private readonly Context _context;
        private static readonly string _dbFilePath = "Cache/cache.db";
        private static readonly string _dbResourcePath = "Gatosyocora.VRCPhotoAlbum.Resources.cache.db";

        public DBCacheService()
        {
            if (!File.Exists(_dbFilePath))
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_dbResourcePath);

                if (stream == null)
                {
                    throw new Exception($"{_dbResourcePath} is not exists.");
                }

                var db = new byte[stream.Length];
                stream.Read(db, 0, db.Length);

                var directoryName = Path.GetDirectoryName(_dbFilePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.WriteAllBytes(_dbFilePath, db);
            }

            _context = new Context();
        }

        public async Task CreateDBCacheIfNeededAsync(IEnumerable<string> filePaths)
        {
            // DBに存在しないものだけキャッシュ作成を行う
            foreach (var filePath in filePaths.Except(await _context.Photos.Select(p => p.FilePath).ToListAsync()))
            {
                var photo = new Photo
                {
                    FilePath = filePath
                };

                // サムネイル作成
                // TODO: ImageHelperに実装
                photo.Thumbnail = await CreateThumbnailAsync(filePath);

                // metaデータ読み込み
                if (!VrcMetaDataReader.TryRead(filePath, out VrcMetaData meta))
                {
                    // TODO: ファイル名から日付情報設定
                    // photo.Date = 
                }
                else
                {
                    // 日付情報設定
                    photo.Date = meta.Date;

                    // ワールド情報設定
                    if (string.IsNullOrEmpty(meta.World))
                    {
                        photo.World = null;
                    }
                    else
                    {
                        if (!ExistsWorldByWorldName(meta.World, out var world))
                        {
                            photo.World = CreateWorld(meta.World);
                        }
                        else
                        {
                            photo.World = world;
                        }
                    }

                    // 撮影者情報設定
                    if (string.IsNullOrEmpty(meta.Photographer))
                    {
                        photo.Photographer = null;
                    }
                    else
                    {
                        var (exists, photographer) = await ExistsUserByUserNameAsync(meta.Photographer);
                        if (!exists)
                        {
                            photo.Photographer = await CreateUserAsync(meta.Photographer);
                        }
                        else
                        {
                            photo.Photographer = photographer;
                        }
                    }

                    // ユーザ情報設定
                    if (meta.Users.Any())
                    {
                        foreach (var metaUser in meta.Users)
                        {
                            var (exists, user) = await ExistsUserByUserNameAsync(metaUser.UserName);

                            if (!exists)
                            {
                                user = await CreateUserAsync(metaUser.UserName);
                            }

                            var photoUser = CreatePhotoUser(photo, user);

                            photo.PhotoUsers.Add(photoUser);
                        }
                    }
                }

                await _context.Photos.AddAsync(photo);
                await _context.SaveChangesAsync();
            }
        }

        public VrcMetaData GetVrcMetaDataIfExists(string filePath)
        {
            var photo = _context.Photos.FirstOrDefault(p => p.FilePath == filePath);

            // VrcMetaDataあり
            if (photo.World != null || photo.Date != null || photo.Photographer != null || photo.PhotoUsers.Any())
            {
                var vrcMetaData = new VrcMetaData
                {
                    World = photo.World.WorldName,
                    Date = photo.Date,
                    Photographer = photo.Photographer.UserName
                };

                foreach (var (userName, twitterScreenName) in photo.Users.Select(u => (u.UserName, u.TwitterScreenName)))
                {
                    var user = new KoyashiroKohaku.VrcMetaTool.User(userName)
                    {
                        TwitterScreenName = twitterScreenName
                    };

                    vrcMetaData.Users.Add(user);
                }

                return vrcMetaData;
            }
            // VrcMetaDataなし
            else
            {
                return null;
            }
        }

        public async Task<VrcMetaData> GetVrcMetaDataIfExistsAsync(string filePath)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.FilePath == filePath);

            // VrcMetaDataあり
            if (photo.World != null || photo.Date != null || photo.Photographer != null || photo.PhotoUsers.Any())
            {
                var vrcMetaData = new VrcMetaData
                {
                    World = photo.World.WorldName,
                    Date = photo.Date,
                    Photographer = photo.Photographer.UserName
                };

                foreach (var (userName, twitterScreenName) in photo.Users.Select(u => (u.UserName, u.TwitterScreenName)))
                {
                    var user = new KoyashiroKohaku.VrcMetaTool.User(userName)
                    {
                        TwitterScreenName = twitterScreenName
                    };

                    vrcMetaData.Users.Add(user);
                }

                return vrcMetaData;
            }
            // VrcMetaDataなし
            else
            {
                return null;
            }
        }

        public IQueryable<Photo> Photos =>
            _context.Photos
            .Include(p => p.World)
            .Include(p => p.PhotoUsers)
                .ThenInclude(pu => pu.User);

        public List<Photo> GetAllPhotos() => Photos.ToList();

        public Task<List<Photo>> GetAllPhotosAsync() => Photos.ToListAsync();

        public List<Photo> GetPhotosByUserName(string userName) =>
            Photos
            .Where(p => p.Users.Select(u => u.UserName).Contains(userName))
            .ToList();

        public Task<List<Photo>> GetPhotosByUserNameAsync(string userName) =>
            Photos
            .Where(p => p.Users.Select(u => u.UserName).Contains(userName))
            .ToListAsync();

        public List<Photo> GetPhotosByWorldName(string worldName) =>
            Photos
            .Where(p => p.World.WorldName.Contains(worldName))
            .ToList();

        public Task<List<Photo>> GetPhotosByWorldNameAsync(string worldName) =>
            Photos
            .Where(p => p.World.WorldName.Contains(worldName))
            .ToListAsync();

        // TODO: ImageHelperに移動予定
        private static Task<byte[]> CreateThumbnailAsync(string filePath)
        {
            return Task.Run(() =>
            {
                var originalImage = Image.FromFile(filePath);
                var thumbnailImage = originalImage.GetThumbnailImage(originalImage.Width / 8, originalImage.Height / 8, () => { return false; }, IntPtr.Zero);

                ImageConverter converter = new ImageConverter();
                return converter.ConvertTo(thumbnailImage, typeof(byte[])) as byte[];
            });
        }

        public bool ExistsWorldByWorldName(string worldName, out World world)
        {
            world = _context.Worlds.FirstOrDefault(w => w.WorldName == worldName);
            return world != null;
        }

        public async Task<(bool exists, World world)> ExistsWorldByWorldNameAsync(string worldName)
        {
            var world = await _context.Worlds.FirstOrDefaultAsync(w => w.WorldName == worldName);
            return (world != null, world);
        }

        public World CreateWorld(string worldName)
        {
            var world = new World
            {
                WorldName = worldName
            };

            _context.Worlds.Add(world);

            return world;
        }

        public async Task<World> CreateWorldAsync(string worldName)
        {
            var world = new World
            {
                WorldName = worldName
            };

            await _context.Worlds.AddAsync(world);

            return world;
        }

        public bool ExistsUserByUserName(string userName, out User user)
        {
            user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            return user != null;
        }

        public async Task<(bool exists, User user)> ExistsUserByUserNameAsync(string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            return (user != null, user);
        }

        public User CreateUser(string userName)
        {
            var user = new User
            {
                UserName = userName
            };

            _context.Users.Add(user);

            return user;
        }

        public async Task<User> CreateUserAsync(string userName)
        {
            var user = new User
            {
                UserName = userName
            };

            _ = await _context.Users.AddAsync(user);

            return user;
        }

        public PhotoUser CreatePhotoUser(Photo photo, User user)
        {
            var photoUser = new PhotoUser
            {
                Photo = photo,
                User = user
            };

            _context.PhotoUsers.Add(photoUser);

            return photoUser;
        }
    }
}
