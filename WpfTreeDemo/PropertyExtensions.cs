using System;
using System.Linq.Expressions;

namespace WpfTreeDemo
{
    public static class PropertyExtensions
    {
        public static string GetName<TProp>(this Expression<Func<TProp>> property)
        {
            var expr = property.Body as MemberExpression;
            if (expr == null)
                throw new InvalidOperationException("Invalid property type");
            return expr.Member.Name;
        }
    }
}