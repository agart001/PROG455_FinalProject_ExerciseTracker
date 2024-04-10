using Newtonsoft.Json;
using System.Collections.Generic;

namespace PROG455_FinalProject_ExerciseTracker.Models
{
    public class Exercise
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public enum EDataType
        {
            Unknown,
            Reps,
            Timed
        }

        public EDataType DataType { get; set; }

        public Exercise()
        {
            ID = Hasher.CreateID();
            Name = string.Empty;
            Description = string.Empty;
            DataType = EDataType.Unknown;
        }

        public Exercise(string name, string description, EDataType datatype)
        {
            ID = Hasher.CreateID();
            Name = name;
            Description = description;
            DataType = datatype;
        }
    }

    public class ExerciseData
    {
        public int ID { get; set; }
        public Dictionary<DateTime, object> Data { get; set; }

        public ExerciseData()
        {
            ID = Hasher.CreateID();
            Data = new Dictionary<DateTime, object>();
        }

        public ExerciseData(Dictionary<DateTime, object> data)
        {
            ID = Hasher.CreateID();
            Data = data;
        }
    }

    public class ExerciseConverter : JsonConverter<Exercise>
    {
        public override bool CanWrite => false;

        public override Exercise ReadJson(JsonReader reader, Type objectType, Exercise existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var exercise = existingValue ?? new Exercise();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = reader.Value.ToString();

                    if (propertyName.Equals("ID", StringComparison.OrdinalIgnoreCase))
                    {
                        exercise.ID = reader.ReadAsInt32() ?? exercise.ID;
                    }
                    else if (propertyName.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    {
                        exercise.Name = reader.ReadAsString() ?? exercise.Name;
                    }
                    else if (propertyName.Equals("Description", StringComparison.OrdinalIgnoreCase))
                    {
                        exercise.Description = reader.ReadAsString() ?? exercise.Description;
                    }
                    else if (propertyName.Equals("DataType", StringComparison.OrdinalIgnoreCase))
                    {
                        var enumValue = reader.ReadAsString();
                        exercise.DataType = (Exercise.EDataType)Enum.Parse(typeof(Exercise.EDataType), enumValue, true);
                    }
                }
            }

            return exercise;
        }

        public override void WriteJson(JsonWriter writer, Exercise value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

}
