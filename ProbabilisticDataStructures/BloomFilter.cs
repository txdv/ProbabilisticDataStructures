﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProbabilisticDataStructures;
using System.Security.Cryptography;

namespace ProbabilisticDataStructures
{
    /// <summary>
    /// BloomFilter implements a classic Bloom filter. A bloom filter has a non-zero
    /// probability of false positives and a zero probability of false negatives.
    /// </summary>
    public class BloomFilter : IFilter
    {
        public Buckets Buckets { get; set; }
        private HashAlgorithm Hash { get; set; }
        private uint m { get; set; }
        private uint k { get; set; }
        public uint Count { get; private set; }

        /// <summary>
        /// Creates a new Bloom filter optimized to store n items with a specified target
        /// false-positive rate.
        /// </summary>
        /// <param name="n">Number of items to store.</param>
        /// <param name="fpRate">Desired false positive rate.</param>
        public BloomFilter(uint n, double fpRate)
        {
            var m = ProbabilisticDataStructures.OptimalM(n, fpRate);
            var k = ProbabilisticDataStructures.OptimalK(fpRate);
            Buckets = new Buckets(m, 1);
            Hash = HashAlgorithm.Create("MD5");
            this.m = m;
            this.k = k;
        }

        /// <summary>
        /// Returns the Bloom filter capacity, m.
        /// </summary>
        /// <returns>The Bloom filter capacity, m.</returns>
        public uint Capacity()
        {
            return this.m;
        }

        /// <summary>
        /// Returns the number of hash functions.
        /// </summary>
        /// <returns>The number of hash functions.</returns>
        public uint K()
        {
            return this.k;
        }

        /// <summary>
        /// Returns the current estimated ratio of set bits.
        /// </summary>
        /// <returns>The current estimated ratio of set bits.</returns>
        public double EstimatedFillRatio()
        {
            return 1 - Math.Exp((-(double)this.Count * (double)this.k) / (double)this.m);
        }

        /// <summary>
        /// Returns the ratio of set bits.
        /// </summary>
        /// <returns>The ratio of set bits.</returns>
        public double FillRatio()
        {
            uint sum = 0;
            for (uint i = 0; i < this.Buckets.Count; i++)
            {
                sum += this.Buckets.Get(i);
            }
            return (double)sum / (double)this.m;
        }

        /// <summary>
        /// Will test for membership of the data and returns true if it is a member,
        /// false if not. This is a probabilistic test, meaning there is a non-zero
        /// probability of false positives but a zero probability of false negatives.
        /// </summary>
        /// <param name="data">The data to search for.</param>
        /// <returns>Whether or not the data is maybe contained in the filter.</returns>
        public bool Test(byte[] data)
        {
            var hashKernel = ProbabilisticDataStructures.HashKernel(data, this.Hash);
            var lower = hashKernel.Item1;
            var upper = hashKernel.Item2;

            // If any of the K bits are not set, then it's not a member.
            for (uint i = 0; i < this.k; i++)
            {
                if (this.Buckets.Get((lower + upper * i) % this.m) == 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Will add the data to the Bloom filter. It returns the filter to allow
        /// for chaining.
        /// </summary>
        /// <param name="data">The data to add.</param>
        /// <returns>The filter.</returns>
        public IFilter Add(byte[] data)
        {
            var hashKernel = ProbabilisticDataStructures.HashKernel(data, this.Hash);
            var lower = hashKernel.Item1;
            var upper = hashKernel.Item2;

            // Set the K bits.
            for (uint i = 0; i < this.k; i++)
            {
                this.Buckets.Set((lower + upper * i) % this.m, 1);
            }

            this.Count++;
            return this;
        }

        /// <summary>
        /// Is equivalent to calling Test followed by Add. It returns true if the data is
        /// a member, false if not.
        /// </summary>
        /// <param name="data">The data to test for and add if it doesn't exist.</param>
        /// <returns>Whether or not the data was probably contained in the filter.</returns>
        public bool TestAndAdd(byte[] data)
        {
            var hashKernel = ProbabilisticDataStructures.HashKernel(data, this.Hash);
            var lower = hashKernel.Item1;
            var upper = hashKernel.Item2;
            var member = true;

            // If any of the K bits are not set, then it's not a member.
            for (uint i = 0; i < this.k; i++)
            {
                var idx = (lower + upper * i) % this.m;
                if (this.Buckets.Get(idx) == 0)
                {
                    member = false;
                }
                this.Buckets.Set(idx, 1);
            }

            this.Count++;
            return member;
        }

        /// <summary>
        /// Restores the Bloom filter to its original state. It returns the filter to
        /// allow for chaining.
        /// </summary>
        /// <returns>The reset bloom filter.</returns>
        public BloomFilter Reset()
        {
            this.Buckets.Reset();
            return this;
        }

        /// <summary>
        /// Sets the hashing function used in the filter.
        /// </summary>
        /// <param name="h">The HashAlgorithm to use.</param>
        // TODO: Add SetHash to the IFilter interface?
        public void SetHash(HashAlgorithm h)
        {
            this.Hash = h;
        }
    }
}
