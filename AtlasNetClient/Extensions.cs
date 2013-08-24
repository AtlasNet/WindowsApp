using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AtlasNetClient
{
    static class Extensions
    {
        public static string GetName(this AtlasNodeInfo info)
        {
            return string.Format("{0}:{1} ({2})", info.Host, info.Port, info.Name);
        }

        public static Tuple<string, int> GetDescriptor(this AtlasNodeInfo info)
        {
            return new Tuple<string, int>(info.Host, (int)info.Port);
        }

        public static bool ChangeAndNotify<T>(this PropertyChangedEventHandler handler, ref T field, T value, Expression<Func<T>> memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException("memberExpression");
            }
            var body = memberExpression.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("Lambda must return a property.");
            }
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            var vmExpression = body.Expression as ConstantExpression;
            if (vmExpression != null)
            {
                LambdaExpression lambda = Expression.Lambda(vmExpression);
                Delegate vmFunc = lambda.Compile();
                object sender = vmFunc.DynamicInvoke();

                if (handler != null)
                {
                    handler(sender, new PropertyChangedEventArgs(body.Member.Name));
                }
            }

            field = value;
            return true;
        }
    }
}
