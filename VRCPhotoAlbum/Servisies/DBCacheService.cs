using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Models.Entities;
using Gatosyocora.VRCPhotoAlbum.Views;
using KoyashiroKohaku.VrcMetaTool;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Photo = Gatosyocora.VRCPhotoAlbum.Models.Entities.Photo;
using User = Gatosyocora.VRCPhotoAlbum.Models.Entities.User;

namespace Gatosyocora.VRCPhotoAlbum.Servisies
{
    public class DBCacheService
    {
        private Context _context { get; set; }
        private string _dbFilePath;
        private static readonly string _dbResourcePath = "Gatosyocora.VRCPhotoAlbum.Resources.cache.db";

        private static readonly AsyncLock asyncLock = new AsyncLock();
        private int count = 0;

        public DBCacheService(string databaseFilePath)
        {
            _dbFilePath = databaseFilePath;
            CreateDBCacheIfNeeded();
        }

        public void CreateDBCacheIfNeeded()
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

            if (_context is null)
            {
                _context = new Context(_dbFilePath);
            }
        }

        public async Task CreateDBCacheIfNeededAsync(IEnumerable<string> filePaths)
        {
            // DBに存在しないものだけキャッシュ作成を行う
            foreach (var filePath in filePaths.Except(await _context.Photos.Select(p => p.FilePath).ToListAsync().ConfigureAwait(true)))
            {
                var photo = new Photo
                {
                    FilePath = filePath
                };

                // サムネイル作成
                // サムネイルは初表示時に作成する
                //photo.Thumbnail = await ImageHelper.CreateThumbnailAsync(filePath);

                // metaデータ読み込み
                if (!VrcMetaDataReader.TryRead(filePath, out VrcMetaData meta))
                {
                    // ファイル名から日付を取得
                    photo.Date = MetaDataHelper.GetDateTimeFromPhotoName(filePath);
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
                        var (exists, photographer) = await ExistsUserByUserNameAsync(meta.Photographer).ConfigureAwait(true);
                        if (!exists)
                        {
                            photo.Photographer = await CreateUserAsync(meta.Photographer).ConfigureAwait(true);
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
                            var (exists, user) = await ExistsUserByUserNameAsync(metaUser.UserName).ConfigureAwait(true);

                            // TODO:TwitterのDisplaynameを入れていない
                            if (!exists)
                            {
                                user = await CreateUserAsync(metaUser.UserName).ConfigureAwait(true);
                            }

                            var photoUser = CreatePhotoUser(photo, user);

                            photo.PhotoUsers.Add(photoUser);
                        }
                    }
                }

                await _context.Photos.AddAsync(photo);
                await _context.SaveChangesAsync().ConfigureAwait(true);
            }
        }

        public List<(string filePath, VrcMetaData vrcMetaData)> GetVrcMetaDataIfExists(IEnumerable<string> filePaths)
        {
            using (asyncLock.Lock())
            {
                var photos = Photos
                                .Select(p => new
                                {
                                    p.FilePath,
                                    p.World,
                                    p.Date,
                                    p.Photographer,
                                    p.PhotoUsers,
                                })
                                .Where(p => filePaths.Contains(p.FilePath))
                                .ToList();

                var results = new List<(string filePath, VrcMetaData vrcMetaData)>();

                foreach (var photo in photos)
                {
                    var filePath = photo.FilePath;
                    var vrcMetaData = new VrcMetaData
                    {
                        World = photo.World?.WorldName,
                        Date = photo.Date,
                        Photographer = photo.Photographer?.UserName
                    };

                    foreach (var (userName, twitterScreenName) in photo.PhotoUsers.Select(pu => (pu.User.UserName, pu.User.TwitterScreenName)))
                    {
                        var user = new KoyashiroKohaku.VrcMetaTool.User(userName)
                        {
                            TwitterScreenName = twitterScreenName
                        };

                        vrcMetaData.Users.Add(user);
                    }

                    results.Add((filePath, vrcMetaData));
                }

                return results;
            }
        }

        public async Task<List<(string filePath, VrcMetaData vrcMetaData)>> GetVrcMetaDataIfExistsAsync(IEnumerable<string> filePaths)
        {
            var photos = await Photos
                .Select(p =>
                new
                {
                    p.FilePath,
                    p.World,
                    p.Date,
                    p.Photographer,
                    p.PhotoUsers
                })
                .ToListAsync()
                .ConfigureAwait(true);

            var results = new List<(string filePath, VrcMetaData vrcMetaData)>();

            foreach (var photo in photos)
            {
                var filePath = photo.FilePath;
                var vrcMetaData = new VrcMetaData
                {
                    World = photo.World?.WorldName,
                    Date = photo.Date,
                    Photographer = photo.Photographer?.UserName
                };

                foreach (var (userName, twitterScreenName) in photo.PhotoUsers.Select(pu => (pu.User.UserName, pu.User.TwitterScreenName)))
                {
                    var user = new KoyashiroKohaku.VrcMetaTool.User(userName)
                    {
                        TwitterScreenName = twitterScreenName
                    };

                    vrcMetaData.Users.Add(user);
                }

                results.Add((filePath, vrcMetaData));
            }

            return results;
        }

        public IQueryable<Photo> Photos =>
            _context
            .Photos
            .AsNoTracking()
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

        public bool ExistsWorldByWorldName(string worldName, out World world)
        {
            world = _context.Worlds.FirstOrDefault(w => w.WorldName == worldName);
            return world != null;
        }

        public async Task<(bool exists, World world)> ExistsWorldByWorldNameAsync(string worldName)
        {
            var world = await _context.Worlds.FirstOrDefaultAsync(w => w.WorldName == worldName).ConfigureAwait(true);
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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName).ConfigureAwait(true);
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

        public async Task<PhotoUser> CreatePhotoUserAsync(Photo photo, User user)
        {
            var photoUser = new PhotoUser
            {
                Photo = photo,
                User = user
            };

            await _context.PhotoUsers.AddAsync(photoUser);

            return photoUser;
        }

        public void SaveChanges() => _context.SaveChanges();

        public Task InsertAsync(string filePath, VrcMetaData metaData, CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                var photo = new Photo
                {
                    FilePath = filePath
                };

                photo.Date = metaData.Date;

                using (await asyncLock.LockAsync())
                {
                    if (cancelToken.IsCancellationRequested) return;

                    if (metaData.Photographer != null)
                    {
                        if (!ExistsUserByUserName(metaData.Photographer, out User user))
                        {
                            user = CreateUser(metaData.Photographer);
                        }
                        photo.Photographer = user;
                    }

                    if (metaData.World != null)
                    {
                        if (!ExistsWorldByWorldName(metaData.World, out World world))
                        {
                            world = CreateWorld(metaData.World);
                        }
                        photo.World = world;
                    }

                    if (metaData.Users != null && metaData.Users.Any())
                    {
                        foreach (var metaUser in metaData.Users)
                        {
                            if (!ExistsUserByUserName(metaUser.UserName, out User user))
                            {
                                user = CreateUser(metaUser.UserName);
                            }

                            var photoUser = CreatePhotoUser(photo, user);
                            photo.PhotoUsers.Add(photoUser);
                        }
                    }

                    _context.Photos.Add(photo);
                    count++;

                    if (count >= 100)
                    {
                        _context.SaveChanges();
                        count = 0;
                    }
                }
            }, cancelToken);
        }

        public void Update(string filePath, BitmapImage thumbnailImage)
        {
            // サムネイル画像が登録されていなければ登録する
            // 引数はbyte[]かBitmapかBitmapImage
            // サムネイル画像を作成したときにDB登録のために使用するメソッド
            // Insertと競合するとエラー吐きそうだからQueueに入れる必要がありそう？
            throw new NotImplementedException();
        }

        public BitmapImage GetThumbnailImageByFilePath(string filePath)
        {
            // サムネイル画像を取得する
            // 画像が画面に表示されたときに使用するメソッド
            // なければnullを返す => 作成してUpdate(filePath, thumnailImage)で登録
            throw new NotImplementedException();
        }

        public VrcMetaData GetVrcMetaDataByFilePath(string filePath)
        {
            // ファイルパスからVrcMetaDataを取得するメソッド
            // オンメモリの方式では使用しないので優先度低
            // 画像が選択されたときにプレビュー画面のために情報を取得する
            throw new NotImplementedException();
        }

        public User GetUsersWithDuplicates(string[] filePaths)
        {
            // 引数で与えられたファイルに写ったユーザー一覧を取得する(重複あり)
            // 写真一覧画面で使用するユーザー一覧
            // 五十音順に加え, 写っている枚数順に並べるため枚数情報も必要
            throw new NotImplementedException();
        }

        public async Task DeleteAll()
        {
            await _context.DisposeAsync();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.Delete(_dbFilePath);
        }
    }
}
