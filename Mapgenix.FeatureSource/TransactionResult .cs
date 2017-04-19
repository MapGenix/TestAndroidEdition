using System;
using System.Collections.Generic;

namespace Mapgenix.FeatureSource
{
    /// <summary>Results from a processed transaction.</summary>
    /// <remarks>Holds the results of a transaction such as the number
    /// of succeeded record and the number of failed records, a dictionary with reasons of failing by FeatureId.</remarks>
    [Serializable]
    public class TransactionResult
    {
        private int _totalSuccessCount;
        private int _totalFailureCount;
        private TransactionResultStatus _transactionResultStatus;
        private readonly Dictionary<string, string> _failureReasons;

        /// <summary>Class constructor.</summary>
        public TransactionResult()
            : this(0, 0, new Dictionary<string, string>(), TransactionResultStatus.Success)
        {
        }

        /// <summary>Class constructor.</summary>
        public TransactionResult(int totalSuccessCount, int totalFailureCount, Dictionary<string, string> failureReasons, TransactionResultStatus transactionResultStatus)
        {
            this._totalSuccessCount = totalSuccessCount;
            this._totalFailureCount = totalFailureCount;
            this._failureReasons = failureReasons;
            this._transactionResultStatus = transactionResultStatus;
        }

        /// <summary>Gets and sets the total number of records committed successfully.</summary>
        /// <returns>Total number of records committed successfully.</returns>
        /// <remarks>None</remarks>
        public int TotalSuccessCount
        {
            get
            {
                return _totalSuccessCount;
            }
            set
            {
                _totalSuccessCount = value;
            }
        }

        /// <summary>Gets and sets the total number of records committed unsuccessfully.</summary>
        /// <returns>Total number of records committed unsuccessfully.</returns>
        /// <remarks>None</remarks>
        public int TotalFailureCount
        {
            get
            {
                return _totalFailureCount;
            }
            set
            {
                _totalFailureCount = value;
            }
        }

        /// <summary>Gets and sets the result status of the transaction.</summary>
        /// <returns>Result status of the transaction.</returns>
        /// <remarks>If all of the records committed fine then a success status is returned. If one or more records failed then a
        /// failure status is returned though some records have committed successfully.</remarks>
        public TransactionResultStatus TransactionResultStatus
        {
            get
            {
                return _transactionResultStatus;
            }
            set
            {
                _transactionResultStatus = value;
            }
        }

        /// <summary>Gets and sets the dictionary with the reasons for failure.</summary>
        /// <returns>Dictionary with reasons for failure.</returns>
        /// <remarks>It is recommended to use the FeatureId as the key of the Dictionary.</remarks>
        public Dictionary<string, string> FailureReasons
        {
            get
            {
                return _failureReasons;
            }
        }
    }
}
