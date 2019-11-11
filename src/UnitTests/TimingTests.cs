﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NTMiner;

namespace UnitTests {
    [TestClass]
    public class TimingTests {
        [TestMethod]
        public void TestMethod1() {
            int secondCount = 1000000;
            Write.Stopwatch.Start();
            int count1;
            Method1(secondCount, out count1);
            var elapsedMilliseconds = Write.Stopwatch.Stop();
            Console.WriteLine("Method1 " + elapsedMilliseconds);
            Write.Stopwatch.Start();
            int count2;
            Method2(secondCount, out count2);
            elapsedMilliseconds = Write.Stopwatch.Stop();
            Console.WriteLine("Method2 " + elapsedMilliseconds);
            Assert.AreEqual(count1, count2);
        }

        [TestMethod]
        public void TestMethod2() {
            int secondCount = 1000000;
            Write.Stopwatch.Start();
            int count1;
            A(secondCount, out count1);
            var elapsedMilliseconds = Write.Stopwatch.Stop();
            Console.WriteLine("A " + elapsedMilliseconds);
            Write.Stopwatch.Start();
            int count2;
            B(secondCount, out count2);
            elapsedMilliseconds = Write.Stopwatch.Stop();
            Console.WriteLine("B " + elapsedMilliseconds);
            Assert.AreEqual(count1, count2);
        }

        private void Method2(int secondCount, out int count) {
            count = 0;
            const int daySecond = 24 * 60 * 60;
            for (int i = 0; i < secondCount; i++) {
                if (secondCount % 2 == 0) {
                    count++;
                    if (secondCount % 10 == 0) {
                        count++;
                        if (secondCount % 20 == 0) {
                            count++;
                            if (secondCount % 60 == 0) {
                                count++;
                                if (secondCount % 120 == 0) {
                                    count++;
                                    if (secondCount % 600 == 0) {
                                        count++;
                                        if (secondCount % 1200 == 0) {
                                            count++;
                                            if (secondCount % 6000 == 0) {
                                                count++;
                                            }
                                            if (secondCount % daySecond == 0) {
                                                count++;
                                            }
                                        }
                                        if (secondCount % 3000 == 0) {
                                            count++;
                                        }
                                    }
                                }
                                if (secondCount % 300 == 0) {
                                    count++;
                                }
                            }
                        }
                    }
                }
                if (secondCount % 5 == 0) {
                    count++;
                }
            }
        }

        private void Method1(int secondCount, out int count) {
            count = 0;
            const int daySecond = 24 * 60 * 60;
            for (int i = 0; i < secondCount; i++) {
                if (secondCount % 2 == 0) {
                    count++;
                }
                if (secondCount % 5 == 0) {
                    count++;
                }
                if (secondCount % 10 == 0) {
                    count++;
                }
                if (secondCount % 20 == 0) {
                    count++;
                }
                if (secondCount % 60 == 0) {
                    count++;
                }
                if (secondCount % 120 == 0) {
                    count++;
                }
                if (secondCount % 300 == 0) {
                    count++;
                }
                if (secondCount % 600 == 0) {
                    count++;
                }
                if (secondCount % 1200 == 0) {
                    count++;
                }
                if (secondCount % 3000 == 0) {
                    count++;
                }
                if (secondCount % 6000 == 0) {
                    count++;
                }
                if (secondCount % daySecond == 0) {
                    count++;
                }
            }
        }

        private void B(int secondCount, out int count) {
            count = 0;
            const int daySecond = 24 * 60 * 60;
            for (int i = 0; i < secondCount; i++) {
                if (secondCount <= 20) {
                    if (secondCount == 1) {
                        count++;
                    }
                    if (secondCount == 2) {
                        count++;
                    }
                    if (secondCount == 5) {
                        count++;
                    }
                    if (secondCount == 10) {
                        count++;
                    }
                    if (secondCount == 20) {
                        count++;
                    }
                }
                else if (secondCount <= 6000) {
                    if (secondCount == 60) {
                        count++;
                    }
                    if (secondCount == 120) {
                        count++;
                    }
                    if (secondCount == 300) {
                        count++;
                    }
                    if (secondCount == 600) {
                        count++;
                    }
                    if (secondCount == 1200) {
                        count++;
                    }
                    if (secondCount == 3000) {
                        count++;
                    }
                    if (secondCount == 6000) {
                        count++;
                    }
                }
                else if (secondCount <= daySecond) {
                    if (secondCount == daySecond) {
                        count++;
                    }
                }
            }
        }

        private void A(int secondCount, out int count) {
            count = 0;
            const int daySecond = 24 * 60 * 60;
            for (int i = 0; i < secondCount; i++) {
                if (secondCount == 1) {
                    count++;
                }
                if (secondCount == 2) {
                    count++;
                }
                if (secondCount == 5) {
                    count++;
                }
                if (secondCount == 10) {
                    count++;
                }
                if (secondCount == 20) {
                    count++;
                }
                if (secondCount == 60) {
                    count++;
                }
                if (secondCount == 120) {
                    count++;
                }
                if (secondCount == 300) {
                    count++;
                }
                if (secondCount == 600) {
                    count++;
                }
                if (secondCount == 1200) {
                    count++;
                }
                if (secondCount == 3000) {
                    count++;
                }
                if (secondCount == 6000) {
                    count++;
                }
                if (secondCount == daySecond) {
                    count++;
                }
            }
        }
    }
}
