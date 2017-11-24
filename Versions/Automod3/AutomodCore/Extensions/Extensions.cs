using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Automod.Extensions
{
    public static class Extensions
    {
        public static bool ContainsMulti(this string ToCheck, params string[] CheckForThese)
        {
            foreach (string CurrentlyChecked in CheckForThese)
            {
                if (ToCheck.Contains(CurrentlyChecked))
                {
                    return true;
                }
            }

            return false;
        }

        //I'd use a template for this, but you can't use == on two templated types.
        //I don't know much about generics in C#, but maybe an IEquatable would work? Not sure.
        public static bool EqualsMulti(this ulong ToCheck, params ulong[] CheckForThese)
        {
            foreach (ulong BeingChecked in CheckForThese)
            {
                if (BeingChecked == ToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EqualsMulti(this string ToCheck, params string[] CheckForThese)
        {
            foreach (string BeingChecked in CheckForThese)
            {
                if (BeingChecked == ToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        public static string RemoveCharacters(this string ToCheck, string CharsToRemove)
        {
            string BeingChecked = ToCheck;
            Char[] RemoveChars = CharsToRemove.ToCharArray();

            foreach (Char CurrentlyRemoved in RemoveChars)
            {
                BeingChecked = BeingChecked.Replace(CurrentlyRemoved.ToString(), "");
            }

            return BeingChecked;
        }

        public static bool IsAllUpper(this string input)
        {
            if (input.Length < 7)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    if (Char.IsLetter(input[i]) && Char.IsLower(input[i]))
                        return false;
                }
                return true;
            }

            else
            {
                int numberOfUppers = 0;

                for (int i = 0; i < input.Length; i++)
                {
                    if (Char.IsLetter(input[i]) && Char.IsUpper(input[i]))
                        numberOfUppers++;
                }

                if (numberOfUppers >= input.Length - 3)
                    return true;
                else
                    return false;
            }
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
    }
}