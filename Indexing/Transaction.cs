﻿using System;

namespace Indexing
{
    public class Transaction
    {
        public Metric Metric { get; set; }

        public string Environment { get; set; }

        public string Machine => System.Environment.MachineName;

        public string Version { get; set; }

        public bool IsSuccess { get; set; }

        public Exception Exception { get; set; }

        public string ResponseCode { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}
