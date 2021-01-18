using System;

namespace RuriLib.Providers.RandomNumbers
{
    public interface IRNGProvider
    {
        Random GetNew();
    }
}
