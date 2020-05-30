using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public interface IPhotoRepository
    {
        Photo GetPhoto(int Id);

        Photo FindPhoto(string uri);
        IEnumerable<Photo> GetAllPhotos();
        Photo Add(Photo photo);
        Photo Update(Photo photoChanges);
        Photo Delete(int id);

    }
}
