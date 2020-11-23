using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TravianTelegramBot.Serializers
{
    public class MongoDbDateTimeSerializer : DateTimeSerializer
    {
        //  MongoDB returns datetime as DateTimeKind.Utc, which cann't be used in our timezone conversion logic
        //  We overwrite it to be DateTimeKind.Unspecified
        public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            //long ticks;
            //TimeSpan offset;

            BsonType bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.String:
                    var stringValue = bsonReader.ReadString();
                    return DateTimeOffset.Parse(stringValue, DateTimeFormatInfo.InvariantInfo).DateTime;

                case BsonType.DateTime:
                    var dateTimeValue = bsonReader.ReadDateTime();
                    return DateTimeOffset.FromUnixTimeMilliseconds(dateTimeValue).DateTime;

                default:
                    throw CreateCannotDeserializeFromBsonTypeException(bsonType);
            }
        }

        ////  MongoDB returns datetime as DateTimeKind.Utc, which cann't be used in our timezone conversion logic
        ////  We overwrite it to be DateTimeKind.Unspecified
        //public override object Deserialize(MongoDB.Bson.IO.BsonReader bsonReader, Type nominalType, Type actualType, MongoDB.Bson.Serialization.IBsonSerializationOptions options)
        //{
        //    var obj = base.Deserialize(bsonReader, nominalType, actualType, options);
        //    var dt = (DateTime)obj;
        //    return new DateTime(dt.Ticks, DateTimeKind.Unspecified);
        //}

        ////  MongoDB stores all datetime as Utc, any datetime value DateTimeKind is not DateTimeKind.Utc, will be converted to Utc first
        ////  We overwrite it to be DateTimeKind.Utc, becasue we want to preserve the raw value
        //public override void Serialize(MongoDB.Bson.IO.BsonWriter bsonWriter, System.Type nominalType, object value, MongoDB.Bson.Serialization.IBsonSerializationOptions options)
        //{
        //    var dt = (DateTime)value;
        //    var utcValue = new DateTime(dt.Ticks, DateTimeKind.Utc);
        //    base.Serialize(bsonWriter, nominalType, utcValue, options);
        //}
    }
}
