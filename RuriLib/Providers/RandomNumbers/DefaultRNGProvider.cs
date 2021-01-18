using System;

namespace RuriLib.Providers.RandomNumbers
{
    public class DefaultRNGProvider : IRNGProvider
    {
        private readonly Random random = new();

        public Random GetNew()
            => new(random.Next(0, int.MaxValue));
    }
}
