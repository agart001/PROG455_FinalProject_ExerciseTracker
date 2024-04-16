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

    public interface IExerciseData
    {
        int ID { get; set; }
        IDictionary<DateTime, object> Data { get; set; }

        string GenericTypeName();
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

    public class ExerciseDataWrapper<T> : IExerciseData where T : notnull
    {
        private ExerciseData<T> _exerciseData;

        public int ID
        {
            get => _exerciseData.ID;
            set => _exerciseData.ID = value;
        }

        public IDictionary<DateTime, object> Data
        {
            get => _exerciseData.Data.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
            set => _exerciseData.Data = value.ToDictionary(kvp => kvp.Key, kvp => (T)kvp.Value);
        }

        public ExerciseDataWrapper(ExerciseData<T> exerciseData)
        {
            _exerciseData = exerciseData;
        }

        public string GenericTypeName() => typeof(T).Name;
    }

    public interface IExerciseDataForm
    {
        object Data { get; set; }

        DateTime Date { get; set; }

        string GenericTypeName();
    }

    public struct ExerciseDateForm<T> : IExerciseDataForm where T : notnull
    {
        private T data;
        public object Data
        {
            get => data;
            set => data = (T)value;
        }

        public DateTime Date { get; set; }

        public string GenericTypeName() => typeof(T).Name;
    }

}
