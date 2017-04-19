using System;

namespace Mapgenix.FeatureSource
{
    /// <summary>Event arguments passed in through the CommittedTransaction and CommittingTransaction events.</summary>
    [Serializable]
    public class CommitTransactionEventArgs : EventArgs
    {
        private TransactionBuffer transactionBuffer;
        private bool cancel;

        /// <summary>Constructor of the event arguments.</summary>
        /// <overloads>Passes in a transaction buffer.</overloads>
        /// <returns>None</returns>
        /// <param name="transactionBuffer">Transaction buffer that has been committed.</param>
        public CommitTransactionEventArgs(TransactionBuffer transactionBuffer)
        {
            this.transactionBuffer = transactionBuffer;
        }

        /// <summary>Constructor of the event arguments.</summary>
        /// <overloads>Default constructor.</overloads>
        /// <returns>None</returns>
        /// <remarks>None</remarks>
        public CommitTransactionEventArgs()
            : this(new TransactionBuffer())
        {
        }

        public bool Cancel
        {
            get { return this.cancel; }
            set { this.cancel = value; }
        }

        /// <summary>Returns the transaction buffer that has been committed.</summary>
        /// <decimalDegreesValue>Transaction buffer that has been committed.</decimalDegreesValue>
        /// <remarks>None</remarks>
        public TransactionBuffer TransactionBuffer
        {
            get
            {
                return this.transactionBuffer;
            }
            set
            {
                this.transactionBuffer = value;
            }
        }
    }
}
