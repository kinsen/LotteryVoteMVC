using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Utility
{
    public delegate bool Spec<T>(T candidate);
    public static class SpecExtensitions
    {
        public static Spec<T> And<T>(this Spec<T> one, Spec<T> other)
        {

            return candidate => one(candidate) && other(candidate);

        }

        public static Spec<T> Or<T>(this Spec<T> one, Spec<T> other)
        {

            return candidate => one(candidate) || other(candidate);

        }

        public static Spec<T> Not<T>(this Spec<T> one)
        {
            return candidate => !one(candidate);
        }
    }
}
