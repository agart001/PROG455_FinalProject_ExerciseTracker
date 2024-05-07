using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace PROG455_FinalProject_ExerciseTracker.Models
{
    #region Classes

    #region Exercise

    /// <summary>
    /// Represents an exercise.
    /// </summary>
    public class Exercise
    {
        /// <summary>
        /// Gets or sets the unique ID of the exercise.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the name of the exercise.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the exercise.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Represents the type of data associated with the exercise.
        /// </summary>
        public enum EDataType
        {
            /// <summary>
            /// Unknown data type.
            /// </summary>
            Unknown,
            /// <summary>
            /// Represents data in terms of repetitions.
            /// </summary>
            Reps,
            /// <summary>
            /// Represents data in terms of time.
            /// </summary>
            Timed
        }

        /// <summary>
        /// Gets or sets the data type of the exercise.
        /// </summary>
        public EDataType DataType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Exercise"/> class with default values.
        /// </summary>
        public Exercise()
        {
            ID = Hasher.CreateID();
            Name = string.Empty;
            Description = string.Empty;
            DataType = EDataType.Unknown;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Exercise"/> class with specified parameters.
        /// </summary>
        /// <param name="name">The name of the exercise.</param>
        /// <param name="description">The description of the exercise.</param>
        /// <param name="datatype">The data type of the exercise.</param>
        public Exercise(string name, string description, EDataType datatype)
        {
            ID = Hasher.CreateID();
            Name = name;
            Description = description;
            DataType = datatype;
        }
    }

    #endregion

    #region ExerciseData
    /// <summary>
    /// Represents the data associated with an exercise.
    /// </summary>
    /// <typeparam name="T">The type of data associated with the exercise.</typeparam>
    public class ExerciseData<T> where T : notnull
    {
        /// <summary>
        /// Gets or sets the unique ID of the exercise data.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the data associated with the exercise.
        /// </summary>
        public Dictionary<DateTime, T> Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseData{T}"/> class with default values.
        /// </summary>
        public ExerciseData()
        {
            ID = Hasher.CreateID();
            Data = new Dictionary<DateTime, T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseData{T}"/> class with specified data.
        /// </summary>
        /// <param name="data">The data associated with the exercise.</param>
        public ExerciseData(Dictionary<DateTime, T> data)
        {
            ID = Hasher.CreateID();
            Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseData{T}"/> class with specified ID and data.
        /// </summary>
        /// <param name="id">The unique ID of the exercise data.</param>
        /// <param name="data">The data associated with the exercise.</param>
        public ExerciseData(int id, Dictionary<DateTime, T> data)
        {
            ID = id;
            Data = data;
        }
    }

    #endregion

    #region ExerciseDataWrapper

    /// <summary>
    /// Wraps the <see cref="ExerciseData{T}"/> class to work with interface <see cref="IExerciseData"/>.
    /// </summary>
    /// <typeparam name="T">The type of data associated with the exercise.</typeparam>
    public class ExerciseDataWrapper<T> : IExerciseData where T : notnull
    {
        private ExerciseData<T> _exerciseData;

        /// <summary>
        /// Gets or sets the unique ID of the exercise data.
        /// </summary>
        public int ID
        {
            get => _exerciseData.ID;
            set => _exerciseData.ID = value;
        }

        /// <summary>
        /// Gets or sets the data associated with the exercise.
        /// </summary>
        public IDictionary<DateTime, object> Data
        {
            get => _exerciseData.Data.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
            set => _exerciseData.Data = value.ToDictionary(kvp => kvp.Key, kvp => (T)kvp.Value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseDataWrapper{T}"/> class with specified exercise data.
        /// </summary>
        /// <param name="exerciseData">The exercise data to wrap.</param>
        public ExerciseDataWrapper(ExerciseData<T> exerciseData)
        {
            _exerciseData = exerciseData;
        }

        /// <summary>
        /// Returns the generic type name of the exercise data.
        /// </summary>
        /// <returns>The generic type name of the exercise data.</returns>
        public string GenericTypeName() => typeof(T).Name;
    }

    #endregion

    #endregion

    #region Interfaces

    #region IExerciseData

    /// <summary>
    /// Represents the interface for exercise data.
    /// </summary>
    public interface IExerciseData
    {
        /// <summary>
        /// Gets or sets the unique ID of the exercise data.
        /// </summary>
        int ID { get; set; }

        /// <summary>
        /// Gets or sets the data associated with the exercise.
        /// </summary>
        IDictionary<DateTime, object> Data { get; set; }

        /// <summary>
        /// Returns the generic type name of the exercise data.
        /// </summary>
        /// <returns>The generic type name of the exercise data.</returns>
        string GenericTypeName();
    }

    #endregion

    #region IExerciseDataForm
    /// <summary>
    /// Represents the interface for exercise data form.
    /// </summary>
    public interface IExerciseDataForm
    {
        /// <summary>
        /// Gets or sets the data associated with the exercise.
        /// </summary>
        object Data { get; set; }

        /// <summary>
        /// Gets or sets the date of the exercise data.
        /// </summary>
        DateTime Date { get; set; }

        /// <summary>
        /// Returns the generic type name of the exercise data.
        /// </summary>
        /// <returns>The generic type name of the exercise data.</returns>
        string GenericTypeName();
    }

    #endregion

    #endregion

    #region Structs

    #region ExerciseDataForm

    /// <summary>
    /// Represents a form of exercise data.
    /// </summary>
    /// <typeparam name="T">The type of data associated with the exercise.</typeparam>
    public struct ExerciseDataForm<T> : IExerciseDataForm where T : notnull
    {
        private T data;

        /// <summary>
        /// Gets or sets the data associated with the exercise.
        /// </summary>
        public object Data
        {
            get => data;
            set => data = (T)value;
        }

        /// <summary>
        /// Gets or sets the date of the exercise data.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Returns the generic type name of the exercise data.
        /// </summary>
        /// <returns>The generic type name of the exercise data.</returns>
        public string GenericTypeName() => typeof(T).Name;
    }

    #endregion

    #endregion
}
