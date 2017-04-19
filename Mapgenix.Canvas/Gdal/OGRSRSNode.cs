using System;
using System.Collections.Generic;
using System.Text;

namespace Mapgenix.Canvas
{
    internal class OgrsrsNode
    {
        internal static string[] ProjcsRule =
        {
            "PROJCS", "GEOGCS", "PROJECTION", "PARAMETER", "UNIT", "AXIS",
            "AUTHORITY", null
        };

        internal static string[] DatumRule = {"DATUM", "SPHEROID", "TOWGS84", "AUTHORITY", null};
        internal static string[] GeogcsRule = {"GEOGCS", "DATUM", "PRIMEM", "UNIT", "AXIS", "AUTHORITY", null};
        internal static List<string[]> OrderingRules = new List<string[]> {ProjcsRule, GeogcsRule, DatumRule, null};
        internal List<OgrsrsNode> ChildNodes;

        internal OgrsrsNode ParentNode;

        internal string Value;

        internal OgrsrsNode()
            : this(string.Empty)
        {
        }

        internal OgrsrsNode(string nodeValue)
        {
            Value = nodeValue;
            ChildNodes = new List<OgrsrsNode>();
        }
        
        private OgrsrsNode GetChild(int index)
        {
            if (ChildNodes != null && ChildNodes.Count >= index && ChildNodes.Count > 0)
                return ChildNodes[index];
            return null;
        }

        internal OgrsrsNode GetNode(string nodeName)
        {
            if (ChildNodes.Count > 0 && string.Equals(Value, nodeName, StringComparison.OrdinalIgnoreCase))
                return this;

            foreach (var childNode in ChildNodes)
            {
                if (childNode.ChildNodes.Count > 0 &&
                    string.Equals(childNode.Value, nodeName, StringComparison.OrdinalIgnoreCase))
                {
                    return childNode;
                }
            }

            foreach (var childNode in ChildNodes)
            {
                var returnNode = childNode.GetNode(nodeName);
                if (returnNode != null)
                    return returnNode;
            }
            return null;
        }

        internal int FindChild(string nodeValue)
        {
            for (var i = 0; i < ChildNodes.Count; i++)
            {
                if (string.Equals(ChildNodes[i].Value, nodeValue, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        internal void SetParentNode()
        {
            foreach (var item in ChildNodes)
            {
                if (item.ChildNodes.Count > 0)
                    item.SetParentNode();
                item.ParentNode = this;
            }
        }

        internal OgrsrsNode Clone()
        {
            var nodeValue = new char[Value.Length];
            Value.CopyTo(0, nodeValue, 0, Value.Length);

            var node = new OgrsrsNode(new string(nodeValue));

            for (var i = 0; i < ChildNodes.Count; i++)
            {
                node.ChildNodes.Add(ChildNodes[i].Clone());
            }
            node.SetParentNode();
            return node;
        }

        private bool NeedsQuoting()
        {
            if (ChildNodes.Count != 0)
                return false;

            if (Value.StartsWith("e", StringComparison.OrdinalIgnoreCase))
                return true;

            if (ParentNode != null && string.Equals(ParentNode.Value, "AUTHORITY"))
                return true;

            if (ParentNode != null && string.Equals(ParentNode.Value, "AXIS") && this != ParentNode.ChildNodes[0])
                return false;
            for (var i = 0; i < Value.Length; i++)
            {
                if ((!char.IsDigit(Value[i]) && Value[i] != '.' && Value[i] != '-' && Value[i] != 'e' && Value[i] != 'E'))
                    return true;
            }
            return false;
        }

        internal string ExportToWkt()
        {
            var wktBuilder = new StringBuilder();
            var childrenWkt = new string[ChildNodes.Count + 1];
            for (var i = 0; i < ChildNodes.Count; i++)
            {
                childrenWkt[i] = ChildNodes[i].ExportToWkt();
            }

            if (NeedsQuoting())
            {
                wktBuilder.AppendFormat("\"{0}\"", Value);
            }
            else
                wktBuilder.Append(Value);

            if (ChildNodes.Count > 0)
                wktBuilder.Append("[");
            for (var i = 0; i < ChildNodes.Count; i++)
            {
                wktBuilder.Append(childrenWkt[i]);
                if (i == ChildNodes.Count - 1)
                    wktBuilder.Append("]");
                else
                    wktBuilder.Append(",");
            }
            return wktBuilder.ToString();
        }

        internal void ImportFromWkt(char[] wktCharArray, ref int processedIndex)
        {
            ChildNodes = new List<OgrsrsNode>();

            var isInQuote = false;

            var buffer = new char[512];

            var bufferIndex = 0;

            while (processedIndex < wktCharArray.Length - 1 && wktCharArray[processedIndex] != '\0')
            {
                if (wktCharArray[processedIndex] == '"')
                {
                    isInQuote = !isInQuote;
                }
                else if (!isInQuote &&
                         (wktCharArray[processedIndex] == ',' || wktCharArray[processedIndex] == '[' ||
                          wktCharArray[processedIndex] == ']' || wktCharArray[processedIndex] == '(' ||
                          wktCharArray[processedIndex] == ')'))
                {
                    break;
                }
                else if (!isInQuote &&
                         (wktCharArray[processedIndex] == ' ' || wktCharArray[processedIndex] == '\t' ||
                          wktCharArray[processedIndex] == (char) 10 || wktCharArray[processedIndex] == (char) 13))
                {
                }
                else
                {
                    buffer[bufferIndex++] = wktCharArray[processedIndex];
                }
                processedIndex++;
            }
           
            Value = new string(buffer, 0, bufferIndex);

            if (wktCharArray[processedIndex] == '[' || wktCharArray[processedIndex] == '(')
            {
                do
                {
                    processedIndex++;
                    var node = new OgrsrsNode();

                    node.ImportFromWkt(wktCharArray, ref processedIndex);
                    ChildNodes.Add(node);
                } while (wktCharArray[processedIndex] == ',');
                if (wktCharArray[processedIndex] != ')' && wktCharArray[processedIndex] != ']')
                    throw new FormatException(string.Format("Unsupported format of WKT near position {0}",
                        processedIndex));
                processedIndex++;
            }
        }

        internal void MakeValueSafe()
        {
            for (var i = 0; i < ChildNodes.Count; i++)
            {
                GetChild(i).MakeValueSafe();
            }

            if (Value[0] >= '0' && Value[0] <= '9' || Value[0] != '.')
                return;

            for (var i = 0; i < Value.Length; i++)
            {
                if (!char.IsLetterOrDigit(Value[i]))
                {
                    Value = Value.Replace(Value[i], '_');
                }
            }
            Value = Value.Replace("__", "_");
        }

        internal void ApplyRemapper(string node, string[] mappingTable, int srcIndex, int dstIndex, int stepSize)
        {
            ApplyRemapper(node, mappingTable, srcIndex, dstIndex, stepSize, false);
        }

        internal void ApplyRemapper(string node, string[] mappingTable, int srcIndex, int dstIndex, int stepSize,
            bool childOfHit)
        {
            if (childOfHit || string.IsNullOrEmpty(node))
            {
                for (var i = 0; mappingTable[i + srcIndex] != null; i += stepSize)
                {
                    if (string.Equals(mappingTable[i + srcIndex], Value, StringComparison.OrdinalIgnoreCase))
                    {
                        Value = mappingTable[i + dstIndex];
                        break;
                    }
                }
            }
            if (node != null)
            {
                childOfHit = string.Equals(Value, node);
            }
            foreach (var childNode in ChildNodes)
            {
                childNode.ApplyRemapper(node, mappingTable, srcIndex, dstIndex, stepSize, childOfHit);
            }
        }

        internal void StripNodes(string nodeValue)
        {
            while (FindChild(nodeValue) >= 0)
            {
                ChildNodes.RemoveAt(FindChild(nodeValue));
            }
            for (var i = 0; i < ChildNodes.Count; i++)
            {
                ChildNodes[i].StripNodes(nodeValue);
            }
        }

        internal void FixupOrdering()
        {
            foreach (var childNode in ChildNodes)
            {
                childNode.FixupOrdering();
            }
            if (ChildNodes.Count < 3)
                return;

            string[] orderRuleToApply = null;
            for (var i = 0; OrderingRules[i] != null; i++)
            {
                if (string.Equals(OrderingRules[i][0], Value))
                {
                    orderRuleToApply = OrderingRules[i];
                    break;
                }
            }
            if (orderRuleToApply == null)
                return;

            #region OriginalSorting

            var panChildKey = new int[ChildNodes.Count];

            for (var i = 1; i < ChildNodes.Count; i++)
            {
                panChildKey[i] = CslFindString(orderRuleToApply, ChildNodes[i].Value);
                if (panChildKey[i] == -1)
                {
                }
            }

            var changed = true;
            for (var i = 1; changed && i < ChildNodes.Count - 1; i++)
            {
                changed = false;
                for (var j = 1; j < ChildNodes.Count - 1; j++)
                {
                    if (panChildKey[j] == -1 || panChildKey[j + 1] == -1)
                        continue;

                    if (panChildKey[j] > panChildKey[j + 1])
                    {
                        var tempNode = ChildNodes[j];

                        ChildNodes[j] = ChildNodes[j + 1];
                        ChildNodes[j + 1] = tempNode;

                        var tempKey = panChildKey[j];
                        panChildKey[j] = panChildKey[j + 1];
                        panChildKey[j + 1] = tempKey;

                        changed = true;
                    }
                }
            }

            #endregion
        }

        private int CslFindString(string[] list, string target)
        {
            if (list == null)
            {
                return -1;
            }
            for (var i = 0; list[i] != null; i++)
            {
                if (string.Equals(list[i], target, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }
    }
}