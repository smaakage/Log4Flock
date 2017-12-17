using System;
using System.Text;
using log4net;

namespace Log4FlockTesting {
    internal class Program
    {
        static void Main(string[] args) {
            log4net.Config.XmlConfigurator.Configure();
            var logger = LogManager.GetLogger(typeof(Program));
            logger.Info("I know he can get the job, but can he do the job?");
            logger.Debug("I'm not arguing that with you.");
            logger.Warn("Be careful!");

            logger.Error("Have you used a computer before?", new FieldAccessException("You can't access this field.", new AggregateException("You can't aggregate this!")));
            try
            {
                new Class1();
            }
            catch (Exception ex)
            {
                logger.Error("I'm afraid I can't do that.", ex);
            }

            logger.Fatal("That's it. It's over.", new EncoderFallbackException("Could not fall backwards."));

            Console.ReadKey();
        }
    }

    internal class Class1
    {
        public Class1() => new Class2();
        private class Class2
        {
            public Class2() => new Class3();
            private class Class3
            {
                public Class3() => new Class4();
                private class Class4
                {
                    public Class4() => new Class5();
                    private class Class5
                    {
                        public Class5() => new Class6();
                        private class Class6
                        {
                            public Class6() => new Class7();
                            private class Class7
                            {
                                public Class7()
                                {
                                    var hi = 1 / int.Parse("0");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
