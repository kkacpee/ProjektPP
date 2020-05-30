using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Models
{
    public class SQLPhotoRepository : IPhotoRepository
    {

        private readonly AppDbContext context;
        private readonly ILogger<SQLPhotoRepository> logger;

        public SQLPhotoRepository(AppDbContext context,
                                    ILogger<SQLPhotoRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }
        public Photo Add(Photo photo)
        {
            context.Photos.Add(photo);
            context.SaveChanges();
            return photo;
        }

        public Photo Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Photo FindPhoto(string uri)
        {
            return context.Photos.FirstOrDefault(c => c.Url == uri);
        }

        public IEnumerable<Photo> GetAllPhotos()
        {
            return context.Photos;
        }

        public Photo GetPhoto(int Id)
        {
            return context.Photos.Find(Id);
            
        }

        public Photo Update(Photo photoChanges)
        {
            throw new NotImplementedException();
        }
    }
}
