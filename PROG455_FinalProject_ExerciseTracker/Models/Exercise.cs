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

    public class ExerciseData<T> where T : notnull
    {
        public int ID { get; set; }
        public Dictionary<DateTime, T> Data { get; set; }

        public ExerciseData()
        {
            ID = Hasher.CreateID();
            Data = new Dictionary<DateTime, T>();
        }

        public ExerciseData(Dictionary<DateTime, T> data)
        {
            ID = Hasher.CreateID();
            Data = data;
        }

        public ExerciseData(int id, Dictionary<DateTime, T> data)
        {
            ID = id;
            Data = data;
        }
    }


}
