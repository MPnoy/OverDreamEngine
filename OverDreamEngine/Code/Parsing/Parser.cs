using System;
using System.Collections.Generic;
using System.Linq;

public static class Parser
{
    public enum NodeType
    {
        NodeRoot, //корень выражения
        NodeWord, //слова без пробелов и кавычек
        NodeQuoted1String, // кавычки ""
        NodeQuoted2String, // кавычки ''
        NodeRoundBracket, // скобки ()
        NodeSquareBracket, // скобки []
        NodeEnum // перечисление через запятую
    }

    public abstract class Node
    {
        public NodeType nodeType;
        public Node prevNode;
        public string item = "";

        public Node(NodeType tNodeType, Node tPrevNode)
        {
            nodeType = tNodeType;
            prevNode = tPrevNode;
        }
    }

    public class CNodeStr : Node
    {
        public CNodeStr(NodeType tNodeType, Node tPrevNode) : base(tNodeType, tPrevNode)
        {
        }

        public CNodeStr(NodeType tNodeType, Node tPrevNode, string tStr) : base(tNodeType, tPrevNode)
        {
            item = tStr;
        }

        public override string ToString()
        {
            return item;
        }
    }

    public class NodeFunc : Node
    {
        public List<Node> nodes = new List<Node>();

        public Node this[Int32 Index]
        {
            get
            {
                return nodes[Index];
            }
        }

        public Int32 Count
        {
            get
            {
                return nodes.Count;
            }
        }

        public NodeFunc(NodeType nodeType, Node prevNode) : base(nodeType, prevNode)
        {
        }

        public NodeFunc(NodeType nodeType, Node prevNode, string str) : base(nodeType, prevNode)
        {
            item = str;
        }

        public Int32 FindNode(string tItem, NodeType tType)
        {
            for (var i = 0; i <= nodes.Count - 1; i++)
            {
                if (nodes[i].nodeType == tType & nodes[i].item == tItem)
                {
                    return i;
                }
            }
            return -1;
        }

        public override string ToString()
        {
            string Out = item;
            switch (nodeType)
            {
                case NodeType.NodeRoundBracket:
                    {
                        Out += "(";
                        break;
                    }

                case NodeType.NodeSquareBracket:
                    {
                        Out += "[";
                        break;
                    }

                case NodeType.NodeEnum:
                    {
                        if (Out != "")
                        {
                            Out += " ";
                        }

                        break;
                    }
            }
            foreach (var iNode in nodes)
            {
                switch (iNode.nodeType)
                {
                    case NodeType.NodeQuoted1String:
                        {
                            Out += "\"" + iNode.ToString() + "\"";
                            break;
                        }

                    case NodeType.NodeQuoted2String:
                        {
                            Out += "'" + iNode.ToString() + "'";
                            break;
                        }

                    default:
                        {
                            Out += iNode.ToString();
                            break;
                        }
                }
                if (iNode != nodes.Last())
                {
                    switch (nodeType)
                    {
                        case NodeType.NodeEnum:
                            {
                                Out += ", ";
                                break;
                            }

                        default:
                            {
                                Out += " ";
                                break;
                            }
                    }
                }
            }
            switch (nodeType)
            {
                case NodeType.NodeRoundBracket:
                    {
                        Out += ")";
                        break;
                    }

                case NodeType.NodeSquareBracket:
                    {
                        Out += "]";
                        break;
                    }
            }
            return Out;
        }
    }

    public static NodeFunc Parse(List<Tokenizer.Token> tokens)
    {
        NodeFunc nowNode = new NodeFunc(NodeType.NodeRoot, null);
        foreach (var iToken in tokens)
        {
            switch (iToken.tokenType)
            {
                case Tokenizer.TokenType.TokenWord:
                    {
                        nowNode.nodes.Add(new CNodeStr(NodeType.NodeWord, nowNode, ((Tokenizer.TokenStr)iToken).item));
                        break;
                    }

                case Tokenizer.TokenType.TokenQuoted1String:
                    {
                        nowNode.nodes.Add(new CNodeStr(NodeType.NodeQuoted1String, nowNode, ((Tokenizer.TokenStr)iToken).item));
                        break;
                    }

                case Tokenizer.TokenType.TokenQuoted2String:
                    {
                        nowNode.nodes.Add(new CNodeStr(NodeType.NodeQuoted2String, nowNode, ((Tokenizer.TokenStr)iToken).item));
                        break;
                    }

                case Tokenizer.TokenType.TokenRoundBracketOpen:
                    {
                        nowNode.nodes.Add(new NodeFunc(NodeType.NodeRoundBracket, nowNode, nowNode.nodes.Last().item));
                        nowNode.nodes.RemoveAt(nowNode.nodes.Count - 2);
                        nowNode = (NodeFunc)nowNode.nodes.Last();
                        break;
                    }

                case Tokenizer.TokenType.TokenRoundBracketClose:
                    {
                        nowNode = (NodeFunc)nowNode.prevNode;
                        break;
                    }

                case Tokenizer.TokenType.TokenSquareBracketOpen:
                    {
                        nowNode.nodes.Add(new NodeFunc(NodeType.NodeSquareBracket, nowNode, nowNode.nodes.Last().item));
                        nowNode.nodes.RemoveAt(nowNode.nodes.Count - 2);
                        nowNode = (NodeFunc)nowNode.nodes.Last();
                        break;
                    }

                case Tokenizer.TokenType.TokenSquareBracketClose:
                    {
                        nowNode = (NodeFunc)nowNode.prevNode;
                        break;
                    }

                case Tokenizer.TokenType.TokenEnumStart:
                    {
                        if (nowNode.nodes.Any())
                        {
                            if (nowNode.nodes.Count == 0)
                            {
                                nowNode.nodes.Add(new NodeFunc(NodeType.NodeEnum, nowNode, ""));
                            }
                            else
                            {
                                nowNode.nodes.Add(new NodeFunc(NodeType.NodeEnum, nowNode, nowNode.nodes.Last().item));
                                nowNode.nodes.RemoveAt(nowNode.nodes.Count - 2);
                            }
                        }
                        else
                        {
                            nowNode.nodes.Add(new NodeFunc(NodeType.NodeEnum, nowNode, ""));
                        }

                        nowNode = (NodeFunc)nowNode.nodes.Last();
                        break;
                    }

                case Tokenizer.TokenType.TokenEnumEnd:
                    {
                        nowNode = (NodeFunc)nowNode.prevNode;
                        break;
                    }
            }
        }
        return nowNode;
    }

}