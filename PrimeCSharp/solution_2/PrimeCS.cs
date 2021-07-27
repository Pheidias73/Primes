// ---------------------------------------------------------------------------
// PrimeCS.cs : Dave's Garage Prime Sieve in C#
// ---------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace PrimeSieveCS
{
    class PrimeCS
    {
        class prime_sieve
        {
            private readonly int sieveSize = 0;
            // replace BitArray class with own custom bit array
            private readonly byte[] bitArray; //making it readonly so we tell the compiler that the variable reference cant change. around 5% increase in performance
            private Dictionary<int, int> myDict = new Dictionary<int, int>
            {
                { 10 , 4 },                 // Historical data for validating our results - the number of primes
                { 100 , 25 },               // to be found under some limit, such as 168 primes under 1000
                { 1000 , 168 },
                { 10000 , 1229 },
                { 100000 , 9592 },
                { 1000000 , 78498 },
                { 10000000 , 664579 },
                { 100000000 , 5761455 }
            };

            public prime_sieve(int size)
            {
                sieveSize = size;
                // only storing the odd numbers in the array, and as 8 bits per byte, divide the sieve size by 16 to calculate the total number of bytes needed. This needs to be
                // rounded up.
                bitArray = new byte[(this.sieveSize + 16) / 16];
            }

            public int countPrimes()
            {
                int count = 0;
                for (int i = 0; i < this.sieveSize; i++)
                    if (GetBit(i))
                        count++;
                return count;
            }

            public bool validateResults()
            {
                if (myDict.ContainsKey(this.sieveSize))
                    return myDict[this.sieveSize] == this.countPrimes();
                return false;
            }

            private bool GetBit(int index)
            {
                // To check for an even number. Replace modulus (% operator with a bitwise and operator (&). 
                // The % operator needs to divide the numbers and then check the remainder. To check whether a number is odd or even just need to check bit 0 of the number.
                // If bit 0 is 1 then its an odd number, if bit 0 is 0 then its event.
                if ((index & 1) == 0)
                    return false;

                // divide the number by 2 as only storing odd numbers in array of bits.
                index >>= 1;
                // now index by 8 to find which byte we need in the array.
                int offset = index >> 3;
                // bits 0-2 indicate which bit in the byte is needed. Create a mask for that byte.
                byte mask = (byte)(1 << (index & 7));
                // check to see if the bit is populated.
                return (bitArray[offset] & mask) == 0;
            }

            // primeSieve
            // 
            // Calculate the primes up to the specified limit
            // NOTE : You could increase the speed of the code further by storing a local copy of the bitArray in a byte* pointer, as this will ignore bounds checking on settings values in the array.
            //        This would require an unsafe code block so may not within the spirit of using c# as a programming language.
            public void runSieve()
            {
                int factor = 3;
                int q = (int)Math.Sqrt(this.sieveSize);

                // as only dealing with odd numbers divide the sieve size by 2. Use shift operator as faster than divide.
                int recordCount = this.sieveSize >> 1;

                while (factor < q)
                {
                    for (int num = factor >> 1; num <= recordCount; num++)
                    {
                        byte mask = (byte)(1 << (num & 7));

                        if ((bitArray[num >> 3] & mask) == 0)
                        {
                            factor = (num << 1) + 1;
                            break;
                        }
                    }

                    // If marking factor 3, you wouldn't mark 6 (it's a mult of 2) so start with the 3rd instance of this factor's multiple.
                    // We can then step by factor * 2 because every second one is going to be even by definition.
                    // Note that bitArray is only storing odd numbers. That means an increment of "num" by "factor" is actually an increment of 2 * "factor"

                    // use factor * factor as it will use less iterations, and divide result by 2 as only need to process for odd numbers.
                    for (int num = (factor * factor) >> 1; num <= recordCount; num += factor)
                    {
                        // bits 0-2 indicate which bit in the byte is needed.
                        byte mask = (byte)(1 << (num & 7));
                        // divide num by 8 to find the byte within the array. Use an or operator set that bit to true.
                        bitArray[num >> 3] |= mask;
                    }

                    factor += 2;
                }
            }

            public void printResults(bool showResults, double duration, int passes)
            {
                if (showResults)
                    Console.Write("2, ");

                int count = 1;
                for (int num = 3; num <= this.sieveSize; num++)
                {
                    if (GetBit(num))
                    {
                        if (showResults)
                            Console.Write(num + ", ");
                        count++;
                    }
                }
                if (showResults)
                    Console.WriteLine("");

                CultureInfo.CurrentCulture = new CultureInfo("en_US", false);

                Console.WriteLine("Passes: " + passes + ", Time: " + duration + ", Avg: " + (duration / passes) + ", Limit: " + this.sieveSize + ", Count: " + count + ", Valid: " + validateResults());

                // Following 2 lines added by rbergen to conform to drag race output format
                Console.WriteLine();
                Console.WriteLine($"davepl;{passes};{duration:G6};1;algorithm=base,faithful=yes,bits=1");
            }
        }

        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

            var tStart = DateTime.UtcNow;
            var passes = 0;
            prime_sieve sieve = null;

            while ((DateTime.UtcNow - tStart).TotalSeconds < 5)
            {
                sieve = new prime_sieve(1000000);
                sieve.runSieve();
                passes++;
            }

            var tD = DateTime.UtcNow - tStart;
            if (sieve != null)
                sieve.printResults(false, tD.TotalSeconds, passes);
        }
    }
}
