using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Common
{
    /// <summary>
    /// MongoDB操作数据帮助类
    /// Author：Carey
    /// Create Date：2019年4月16日10:01:33
    /// </summary>
    public class MongoDBHelper<T>
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        private IMongoCollection<T> collection;
        public MongoDBHelper(string connectStr, string dbName)
        {
            client = new MongoClient(connectStr);
            db = client.GetDatabase(dbName);
        }

        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <typeparam name="T">Model的类型</typeparam>
        /// <param name="expression">lambda表达式</param>
        /// <returns>查询的结果集合</returns>
        public IList<T> GetInfo(Expression<Func<T, bool>> expression, string collectionName)
        {
            collection = db.GetCollection<T>(collectionName);
            IList<T> resultList = collection.Find(expression).Limit(10).ToList();
            return resultList;
        }


        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <typeparam name="T">Model的类型</typeparam>
        /// <param name="expression">lambda表达式</param>
        /// <returns>查询的结果集合</returns>
        public IList<T> GetInfo(string input, string collectionName)
        {
            collection = db.GetCollection<T>(collectionName);
            var builders = Builders<T>.Filter;
            string tmp = input.Replace("{", "").Replace("}", "");
            string[] arr = tmp.Split(',');
            string[] tmpArr;
            FilterDefinition<T> filter;
            tmpArr = arr[0].Replace("\"", "").Split(':');
            filter = builders.Regex(tmpArr[0], tmpArr[1]);
            for (int i = 1; i < arr.Length; i++)
            {
                tmpArr = arr[i].Replace("\"", "").Split(':');
                filter &= builders.Regex(tmpArr[0], tmpArr[1]);
            }
            IList<T> resultList = collection.Find(filter).ToList();
            return resultList;
        }

        /// <summary>
        /// 插入信息
        /// </summary>
        public IList<T> InsertInfo(IList<T> inputList, string collectionName)
        {
            collection = db.GetCollection<T>(collectionName);

            IList<T> resultList = inputList;
            collection.InsertMany(resultList);

            return resultList;
        }

        /// <summary>
        /// 更新单条记录
        /// </summary>
        /// <param name="filter">lamda表达式</param>
        /// <param name="model">新的Model的List类型</param>
        /// <param name="collectionName">集合名称</param>
        /// <param name="isInert">该值指示如果记录不存在是否进行插入操作 true：是 false：否</param>
        public UpdateResult UpdateOne<T>(Expression<Func<T, bool>> filter, T model, string collectionName = "", bool isInert = false)
        {
            if (collectionName == "")
            {
                collectionName = typeof(T).Name;
            }
            var collection = _database.GetCollection<T>(collectionName);
            var model_update = GetUpdateDefinition(model);
            UpdateResult result = collection.UpdateOne(filter, model_update, new UpdateOptions() { IsUpsert = isInert });
            return result;
        }

        /// <summary>
        /// 更新单条记录
        /// </summary>
        /// <param name="filter">lamda表达式</param>
        /// <param name="model">新的Model的UpdateDefinition类型</param>
        /// <param name="collectionName">集合名称</param>
        /// <param name="isInert">该值指示如果记录不存在是否进行插入操作 true：是 false：否</param>
        public UpdateResult UpdateOne(Expression<Func<T, bool>> filter, UpdateDefinition<T> model, string collectionName,bool isInert)
        {
            collection = db.GetCollection<T>(collectionName);

            UpdateResult result=collection.UpdateOne(filter, model, new UpdateOptions() { IsUpsert = isInert });
            return result;
        }

        public UpdateResult Update(string field,string value,string filterfield, string id, string collectionName)
        {
            collection = db.GetCollection<T>(collectionName);
            var filters =  Builders<T>.Filter.Eq(filterfield, id);
            var update = Builders<T>.Update.Set(field, value);
            //var update = Builders<UpdateDefinition<T>>.Update.Combine(model).ToBsonDocument();
            UpdateResult result = collection.UpdateOne(filters, update);
            return result;
        }

        public UpdateResult Update(string field, string value,string filterfield, Guid id, string collectionName)
        {
            collection = db.GetCollection<T>(collectionName);
            var filters = Builders<T>.Filter.Eq(filterfield, id);
            var update = Builders<T>.Update.Set(field, value);
            UpdateResult result = collection.UpdateOne(filters, update);
            return result;
        }

        public UpdateResult Update(string field, string value,FilterDefinition<T> filters, string collectionName)
        {
            collection = db.GetCollection<T>(collectionName);
            var update = Builders<T>.Update.Set(field, value);
            var result = collection.UpdateOne(filters, update);
            //UpdateResult result = collection.UpdateOne(filters, update);
            return result;
        }

        public UpdateResult GrandUpdate(FilterDefinition<T> filters,UpdateDefinition<T> update,string collectionName)
        {
            collection = db.GetCollection<T>(collectionName);
            var result = collection.UpdateMany(filters, update);
            
            return result;
        }

        private List<UpdateDefinition<BsonDocument>> BuildUpdate(BsonDocument bc,string parent)
        {
            var updates = new List<UpdateDefinition<BsonDocument>>();
            foreach (var element in bc.Elements)
            {
                var key = parent == null ? element.Name : $"{parent}.{element.Name}";
                if (element.Value.IsBsonDocument)
                {
                    updates.AddRange(BuildUpdate(element.Value.ToBsonDocument(), key));
                }
                else if(element.Value.IsBsonArray)
                {
                    var arryDocs = element.Value.AsBsonArray;
                    int i = 0;
                    foreach (var doc in arryDocs)
                    {
                        if (doc.IsBsonDocument)
                        {
                            updates.AddRange(BuildUpdate(doc.ToBsonDocument(), key + $".{i}"));
                        }
                        else
                        {
                            updates.Add(Builders<BsonDocument>.Update.Set(f => f[key], element.Value));
                            continue;
                        }
                        i++;
                    }
                }
                else
                {
                    updates.Add(Builders<BsonDocument>.Update.Set(f => f[key], element.Value));
                }
            }
            return updates;
        }

        /// <summary>
        /// 根据filter数组匹配响应的数据
        /// </summary>
        /// <param name="filterField">filter中field的集合</param>
        /// <param name="filterValue">filter中value的集合</param>
        /// <param name="collectionName">集合名称</param>
        /// <returns></returns>
        public IList<T> GetInfo(string[] filterField,string[] filterValue, string collectionName)
        {
            collection = db.GetCollection<T>(collectionName);
            var builders = Builders<T>.Filter;
            FilterDefinition<T> filter;
            filter = builders.Regex(filterField[0], filterValue[0]);
            for (int i = 1; i < filterField.Length; i++)
            {
                filter &= builders.Regex(filterField[i], filterValue[i]);
            }
            IList<T> resultList = collection.Find(filter).ToList();
            return resultList;
        }

        public IList<T> FindAndUpdate(string[] filterField, string[] filterValue, string collectionName)
        {
            collection = db.GetCollection<T>(collectionName);
            var builders = Builders<T>.Filter;
            FilterDefinition<T> filter;
            filter = builders.Regex(filterField[0], filterValue[0]);
            for (int i = 1; i < filterField.Length; i++)
            {
                filter &= builders.Regex(filterField[0], filterValue[1]);
            }
            IList<T> resultList = collection.Find(filter).ToList();
            //collection.UpdateMany(filter,);
            return resultList;
        }

        /// <summary>
        /// 将一个类型为T的Model对象构造成UpdateDefinition<T>
        /// </summary>
        /// <typeparam name="T">Model的类型</typeparam>
        public UpdateDefinition<T> GetUpdateDefinition<T>(T model)
        {
            UpdateDefinition<T> updates;
            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            List<UpdateDefinition<T>> udList = new List<UpdateDefinition<T>>();
            foreach (var item in properties)
            {
                if (item.PropertyType.IsArray || typeof(IEnumerable).IsAssignableFrom(item.PropertyType))
                {
                    var values = item.GetValue(model) as IList;
                    var fields = item.Name;
                    udList.Add(Builders<T>.Update.Set(fields, values));
                }
                else
                {
                    var values = item.GetValue(model);
                    var fields = item.Name;
                    udList.Add(Builders<T>.Update.Set(fields, values));
                }
            }
            updates = new UpdateDefinitionBuilder<T>().Combine(udList);
            return updates;
        }
    }
}
