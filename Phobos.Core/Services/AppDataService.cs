using CommunityToolkit.HighPerformance;
using Microsoft.Data.Sqlite;
using Dapper;
using Phobos.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Data;

namespace Phobos.Core.Services
{
    [DapperAot]
    public class AppDataService : IAppDataService
    {
        private SqliteConnection db;
        private ObservableCollection<Bookmark> bookmarks;
        public ReadOnlyObservableCollection<Bookmark> Bookmarks { get; }

        public AppDataService(IEnvironmentService environment)
        {
            SqlMapper.AddTypeHandler(new UriMapper());

            db = new SqliteConnection($"Data Source={Path.Combine(environment.DataDirectory, "data.db")}");
            db.Open();
            EnsureVersionAndTables();
            bookmarks = new ObservableCollection<Bookmark>(db.Query<Bookmark>("SELECT * FROM bookmarks"));
            Bookmarks = new ReadOnlyObservableCollection<Bookmark>(bookmarks);
        }

        public void AddBookmark(Bookmark bookmark)
        {
            bookmarks.Add(bookmark);
            db.Execute("INSERT INTO bookmarks VALUES(@Name, @Uri)", new { bookmark.Name, Uri = bookmark.Uri.ToString() });
        }

        public void RemoveBookmark(Bookmark bookmark)
        {
            bookmarks.Remove(bookmark);
            db.Execute("DELETE FROM bookmarks WHERE uri=@Uri", new { Uri = bookmark.Uri.ToString() });
        }

        public CertificateStatus CheckCertificate(Certificate cert)
        {
            var result = db.QueryFirstOrDefault<Certificate>("SELECT * FROM certificates WHERE subject = @Subject", new { cert.Subject });

            if (result is null)
            {
                db.Execute("INSERT INTO certificates VALUES (@Subject, @Thumbprint)", cert);
                return CertificateStatus.New;
            }
            if (!result.Thumbprint.AsSpan().SequenceEqual(cert.Thumbprint))
            {
                return CertificateStatus.Mismatch;
            }
            else
            {
                return CertificateStatus.Ok;
            }
        }

        public void SetCertificate(Certificate certificate)
        {
            db.Execute("""
                INSERT INTO certificates VALUES(@Subject, @Thumbprint)
                ON CONFLICT(subject) DO UPDATE SET thumbprint=@Thumbprint
                """, certificate);
        }

        private void EnsureVersionAndTables()
        {
            db.Execute("CREATE TABLE IF NOT EXISTS info (version INTEGER)");

            var version = db.ExecuteScalar<int?>("SELECT version FROM info") ?? -1;

            // future: upgrade db if incompatible
            // -1 means new db
            if (version < 0)
            {
                db.Execute("INSERT INTO info VALUES(@Version)", new { Version = Constants.DbVersion });

                db.Execute("""
                    CREATE TABLE IF NOT EXISTS bookmarks(name TEXT, uri TEXT PRIMARY KEY);
                    CREATE TABLE IF NOT EXISTS certificates(subject TEXT PRIMARY KEY, thumbprint BLOB);
                    CREATE TABLE IF NOT EXISTS history(uri TEXT);
                    """);
            }
        }


        public void Dispose()
        {
            db.Dispose();
        }

        class UriMapper : SqlMapper.TypeHandler<Uri>
        {
            public override Uri? Parse(object value)
            {
                if (value is not string str)
                    throw new InvalidOperationException("Input must be a string");
                else
                    return new Uri(str);
            }

            public override void SetValue(IDbDataParameter parameter, Uri? value)
            {
                parameter.Value = value.ToString();
            }
        }
    }
}
