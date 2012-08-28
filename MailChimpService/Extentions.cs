using System;
using System.Collections.Generic;
using System.Linq;
using MailChimp;

namespace MailChimpService.MailChimp
{
    internal static class Extentions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }
        public static string Tag(this IEnumerable<MCMergeVar> source, string name)
        {
            return source.Where(merge => merge.name == name).FirstOrDefault().tag;
        }
    }
}