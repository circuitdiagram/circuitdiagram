using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.IO.Data;

namespace CircuitDiagram.Document.InternalWriter
{
    class WriterContext
    {
        private readonly Dictionary<IElement, string> elementIds;
        private readonly Dictionary<ComponentType, string> typeIds;
        private readonly Dictionary<Connection, string> connectionIds; 
        
        private int elementIdCounter;
        private int typeIdCounter;
        private int connectionIdCounter;

        public WriterContext()
        {
            elementIds = new Dictionary<IElement, string>();
            typeIds = new Dictionary<ComponentType, string>();
            connectionIds = new Dictionary<Connection, string>();
        }

        public string GetOrAssignId(IElement element)
        {
            string id;
            if (elementIds.TryGetValue(element, out id))
                return id;

            id = (elementIdCounter++).ToString();
            elementIds.Add(element, id);
            return id;
        }

        public string AssignId(ComponentType type)
        {
            string id = (typeIdCounter++).ToString();
            typeIds.Add(type, id);
            return id;
        }

        public string GetId(ComponentType type)
        {
            return typeIds[type];
        }

        public string GetOrAssignId(Connection connection)
        {
            string id;
            if (connectionIds.TryGetValue(connection, out id))
                return id;

            id = (connectionIdCounter++).ToString();
            connectionIds.Add(connection, id);
            return id;
        }
    }
}
