using System.Data.Entity;

namespace DemoApi.Model
{
    public class DemoApiContext : DbContext
    {
        public  DemoApiContext ()
            : base("DemoApiDatabase")
        {
        }

        public DbSet<CustomerList> CustomerLists { get; set; }

        public DbSet<FileUpload> FileUploads { get; set; }
    }
}