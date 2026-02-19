namespace ComicStoreASP.Data
{
    public class DatatableVersion
    {
        public int Id { get; set; }
        public string VersionName { get; set; }
        public DateTime ImportedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
