using System;
using System.Transactions;
namespace AuthServer.Common
{
    public static class TransactionBuilder
    {
        public static TransactionScope CreateReadCommitted(int miniuts)
        {
            var options = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = new TimeSpan(0,miniuts,0)
            };
            return new TransactionScope(TransactionScopeOption.Required, options);
        } 
        //
        public static TransactionScope CreateSerializable()
        {
            var options = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.Serializable,
                Timeout = TransactionManager.DefaultTimeout
            };
            return new TransactionScope(TransactionScopeOption.Required, options);
        }
        //
        public static TransactionScope CreateReadUncommitted(int miniuts,int seconds=0)
        {
            var options = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted,
                Timeout = new TimeSpan(0,miniuts,seconds)
            };
            return new TransactionScope(TransactionScopeOption.Required, options);
        }
    }
}