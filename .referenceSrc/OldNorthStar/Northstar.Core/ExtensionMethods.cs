using System;
using System.Collections.Generic;

namespace NorthStar.Core
{
    public static class ExtensionMethods
    {
		/// <summary>
		/// Executes the given action against the given ICollection instance.
		/// </summary>
		/// <typeparam name="T">The type of the ICollection parameter.</typeparam>
		/// <param name="items">The collection the action is performed against.</param>
		/// <param name="action">The action that is performed on each item.</param>
		public static void Each<T>(this IEnumerable<T> items, Action<T> action)
		{
			foreach (T item in items)
			{
				action(item);
			}
		}
	}
}