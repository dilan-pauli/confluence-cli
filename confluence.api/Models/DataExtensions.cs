using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace confluence.api.Models
{
    public class DataExtensions
    {
        public static DataTable CreateDataTable<T>()
        {
            var dt = new DataTable();

            var propList = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (MemberInfo info in propList)
            {
                if (info is PropertyInfo && (((PropertyInfo)info).PropertyType.IsPrimitive || ((PropertyInfo)info).PropertyType == typeof(string)))
                {
                    dt.Columns.Add(new DataColumn(info.Name, ((PropertyInfo)info).PropertyType));
                }
            }

            return dt;
        }

        public static void FillDataTable<T>(DataTable dt, List<T> items)
        {
            var propList = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (T t in items)
            {
                var row = dt.NewRow();
                foreach (MemberInfo info in propList)
                {
                    if (info is PropertyInfo && (((PropertyInfo)info).PropertyType.IsPrimitive || ((PropertyInfo)info).PropertyType == typeof(string)))
                        row[info.Name] = ((PropertyInfo)info).GetValue(t, null);
                }
                dt.Rows.Add(row);
            }
        }
    }
}
