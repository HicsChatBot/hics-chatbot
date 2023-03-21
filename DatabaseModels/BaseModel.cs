using System.Text.Json;
using System.Text.Json.Nodes;

namespace HicsChatBot.Model
{
    abstract public record class BaseModel<T> where T : BaseModel<T>
    {
        public abstract object ToObject();

        public static T ToEntity(JsonNode json)
        {
            return null;
        }
    }
}
