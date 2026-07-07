using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DataConcentrator
{
    // DTO za (de)serializaciju taga u JSON (ravna struktura sa poljem Type).
    public class TagDto
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IOAddress { get; set; }
        public int ScanTime { get; set; }
        public bool OnScan { get; set; }
        public double LowLimit { get; set; }
        public double HighLimit { get; set; }
        public string Units { get; set; }
        public double Deadband { get; set; }
        public double Hysteresis { get; set; }
        public double InitialValue { get; set; }
    }

    // F6: export/import konfiguracije svih tagova u JSON (ugradjeni DataContractJsonSerializer).
    public static class ConfigSerializer
    {
        public static string Export(IEnumerable<Tag> tags)
        {
            var dtos = tags.Select(ToDto).ToList();
            var serializer = new DataContractJsonSerializer(typeof(List<TagDto>));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, dtos);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static List<Tag> Import(string json)
        {
            var serializer = new DataContractJsonSerializer(typeof(List<TagDto>));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var dtos = (List<TagDto>)serializer.ReadObject(ms);
                return dtos.Select(FromDto).ToList();
            }
        }

        private static TagDto ToDto(Tag tag)
        {
            var dto = new TagDto
            {
                Type = tag.Type.ToString(),
                Name = tag.Name,
                Description = tag.Description,
                IOAddress = tag.IOAddress
            };

            switch (tag)
            {
                case AnalogInput ai:
                    dto.ScanTime = ai.ScanTime; dto.OnScan = ai.OnScan;
                    dto.LowLimit = ai.LowLimit; dto.HighLimit = ai.HighLimit; dto.Units = ai.Units;
                    dto.Deadband = ai.Deadband; dto.Hysteresis = ai.Hysteresis;
                    break;
                case AnalogOutput ao:
                    dto.LowLimit = ao.LowLimit; dto.HighLimit = ao.HighLimit; dto.Units = ao.Units;
                    dto.InitialValue = ao.InitialValue;
                    break;
                case DigitalInput di:
                    dto.ScanTime = di.ScanTime; dto.OnScan = di.OnScan;
                    break;
                case DigitalOutput dof:
                    dto.InitialValue = dof.InitialValue;
                    break;
            }
            return dto;
        }

        private static Tag FromDto(TagDto dto)
        {
            Tag tag;
            switch (dto.Type)
            {
                case "AI":
                    tag = new AnalogInput
                    {
                        ScanTime = dto.ScanTime, OnScan = dto.OnScan,
                        LowLimit = dto.LowLimit, HighLimit = dto.HighLimit, Units = dto.Units,
                        Deadband = dto.Deadband, Hysteresis = dto.Hysteresis
                    };
                    break;
                case "AO":
                    tag = new AnalogOutput
                    {
                        LowLimit = dto.LowLimit, HighLimit = dto.HighLimit,
                        Units = dto.Units, InitialValue = dto.InitialValue
                    };
                    break;
                case "DI":
                    tag = new DigitalInput { ScanTime = dto.ScanTime, OnScan = dto.OnScan };
                    break;
                case "DO":
                    tag = new DigitalOutput { InitialValue = dto.InitialValue };
                    break;
                default:
                    throw new InvalidOperationException("Nepoznat tip taga: " + dto.Type);
            }

            tag.Name = dto.Name;
            tag.Description = dto.Description;
            tag.IOAddress = dto.IOAddress;
            return tag;
        }
    }
}
