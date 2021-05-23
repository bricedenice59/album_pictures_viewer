using Newtonsoft.Json;
using System;

namespace PhotoApp.Db.Models
{
    public partial class PhotoExif
    {
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public uint? Iso { get; set; }
        public double? ExposureTime { get; set; }
        public double? FNumber { get; set; }
    }

    public partial class PhotoExif
    {
        public static PhotoExif FromJson(string json) => JsonConvert.DeserializeObject<PhotoExif>(json, PhotoExifJsonConverter.Settings);
        public static string ToJson(PhotoExif photoExif) => JsonConvert.SerializeObject(photoExif, Formatting.None, PhotoExifJsonConverter.Settings);

    }

    public static class PhotoExifJsonConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None
        };
    }
}