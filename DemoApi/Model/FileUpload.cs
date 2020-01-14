using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoApi.Model
{
    public class FileUpload
    {
        [Index]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Size { get; set; }
        public string Path { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}