using Phobos.Core.Models;
using System.Collections.ObjectModel;

namespace Phobos.Core.Services
{
    public interface IAppDataService : IDisposable
    {
        ReadOnlyObservableCollection<Bookmark> Bookmarks { get; }
        void AddBookmark(Bookmark bookmark);
        void RemoveBookmark(Bookmark bookmark);

        CertificateStatus CheckCertificate(Certificate cert);
        void SetCertificate(Certificate certificate);
    }
}