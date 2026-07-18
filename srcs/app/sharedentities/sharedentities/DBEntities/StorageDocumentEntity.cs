using System.ComponentModel.DataAnnotations.Schema;

namespace sharedentities.DBEntities
{

    [Table("storagedocument")]
    public class StorageDocument
    {
        public long Id { get; set; }
        public string? DocTypeCode { get; set; }
        public string? DocUrl { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreationDate { get; set; }
        public string? CreationUser { get; set; }
        public DateTime ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
