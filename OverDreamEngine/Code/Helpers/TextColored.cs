using System;
using System.Collections.Generic;

[Serializable]
public class TextColored
{
    [Serializable]
    public struct TextColor
    {
        public int start;
        public int end;
        public SColor color;

        public TextColor(int start, int end, SColor color)
        {
            this.start = start;
            this.end = end;
            this.color = color;
        }

        public TextColor(TextColor textColor)
        {
            start = textColor.start;
            end = textColor.end;
            color = textColor.color;
        }

        public TextColor(TextColor textColor, int offset)
        {
            start = textColor.start + offset;
            end = textColor.end + offset;
            color = textColor.color;
        }
    }

    public string text = "";
    public List<TextColor> textColors = new List<TextColor>();
    public SColor defaultColor;

    public TextColored() : this(new SColor(1, 1, 1)) { }

    public TextColored(SColor color)
    {
        defaultColor = color;
    }

    public TextColored(TextColored text)
    {
        this.text = text.text;
        textColors.AddRange(text.textColors);
        defaultColor = text.defaultColor;
    }

    public TextColored(string text, SColor color)
    {
        this.text = text;
        defaultColor = color;
    }

    public static TextColored operator +(TextColored text1, TextColored text2)
    {
        TextColored ret = new TextColored(text1.text + text2.text, text1.defaultColor);
        ret.textColors.AddRange(text1.textColors);
        if (text1.defaultColor != text2.defaultColor)
        {
            ret.SetColor(text1.text.Length, text2.text.Length, text2.defaultColor);
        }

        text2.textColors.ForEach(item => ret.SetColor(new TextColor(item, text1.text.Length)));
        ret.TestColors();
        return ret;
    }

    public static TextColored operator +(TextColored text1, string text2)
    {
        TextColored ret = new TextColored(text1.text + text2, text1.defaultColor);
        ret.textColors.AddRange(text1.textColors);
        ret.TestColors();
        return ret;
    }

    private void TestColors()
    {
        var prev = 0;
        for (int i = 0; i < textColors.Count; i++)
        {
            if (prev > textColors[i].start)
            {
                throw new Exception();
            }
            prev = textColors[i].start;
            if (prev > textColors[i].end)
            {
                throw new Exception();
            }
            prev = textColors[i].end;
        }
    }

    public void SetColor(TextColor textColor, bool replace = true)
    {
        SetColorBody(textColor.start, textColor.end, textColor.color, replace);
    }

    public void SetColor(int start, int length, SColor color, bool replace = true)
    {
        if (length == 0)
        {
            return;
        }

        SetColorBody(start, start + length - 1, color, replace);
    }

    public void SetColor(SColor color, bool replace = true)
    {
        if (text.Length > 0)
        {
            SetColorBody(0, text.Length - 1, color, replace);
        }
    }

    public TextColored Colorize(SColor color, bool replace = true)
    {
        var ret = new TextColored(this);
        ret.SetColor(color, replace);
        return ret;
    }

    private void SetColorBody(int start, int end, SColor color, bool replace = true)
    {
        void Body(int start1, int end1, SColor color1)
        {
            if (start1 < 0 || end1 < start1)
            {
                throw new Exception();
            }

            bool afterStart = true, afterEnd = true;
            int iStart = -1, iEnd = -1;
            for (int i = 0; i < textColors.Count; i++)
            {
                if (iStart == -1)
                {
                    if (start1 >= textColors[i].start && start1 <= textColors[i].end)
                    {
                        iStart = i;
                        afterStart = false;
                    }
                    else if (i == textColors.Count - 1 || (start1 > textColors[i].end && start1 < textColors[i + 1].start))
                    {
                        iStart = i;
                        afterStart = true;
                    }
                }
                if (iEnd == -1)
                {
                    if (end1 >= textColors[i].start && end1 <= textColors[i].end)
                    {
                        iEnd = i;
                        afterEnd = false;
                    }
                    else if (i == textColors.Count - 1 || (end1 > textColors[i].end && end1 < textColors[i + 1].start))
                    {
                        iEnd = i;
                        afterEnd = true;
                    }
                }
                if ((iStart != -1) && (iEnd != -1))
                {
                    break;
                }
            }
            if (iStart == -1 && iEnd == -1)
            {
                if (text.Length > 0)
                {
                    textColors.Add(new TextColor(start1, end1, color1));
                }

                return;
            }
            var tmpStart = textColors[iStart];
            var tmpEnd = textColors[iEnd];
            if (!afterStart)
            {
                textColors[iStart] = tmpStart.start != start1 ? 
                    new TextColor(tmpStart.start, start1 - 1, tmpStart.color) : 
                    new TextColor();
            }
            for (int i = iStart + 1; i <= iEnd - 1; i++)
            {
                textColors.RemoveAt(iStart + 1);
            }

            if (iEnd - iStart >= 1)
            {
                if (afterEnd || textColors[iStart + 1].end == end1)
                {
                    textColors.RemoveAt(iStart + 1);
                }
                else
                {
                    textColors[iStart + 1] = new TextColor(end1 + 1, textColors[iStart + 1].end, textColors[iStart + 1].color);
                }
            }
            else if (!afterEnd && tmpEnd.end != end1)
            {
                textColors.Insert(iStart + 1, new TextColor(end1 + 1, tmpEnd.end, tmpEnd.color));
            }

            textColors.Insert(iStart + 1, new TextColor(start1, end1, color1));
            if (!afterStart && tmpStart.start == start1)
            {
                textColors.RemoveAt(iStart);
            }
            TestColors();
        }

        if (replace)
        {
            Body(start, end, color);
        }
        else
        {
            var tmpList = new List<TextColor>(textColors);
            textColors.Clear();
            Body(start, end, color);
            tmpList.ForEach(item => Body(item.start, item.end, item.color));
        }
        TestColors();
    }

    public SColor GetColor(int index)
    {
        for (int i = 0; i < textColors.Count; i++)
        {
            if (index < textColors[i].start)
            {
                break;
            }

            if (index <= textColors[i].end)
            {
                return textColors[i].color;
            }
        }
        return defaultColor;
    }

    public string GetTaggedString()
    {
        if (textColors.Count == 0)
        {
            return "<" + defaultColor.ToHTMLString() + ">" + text;
        }
        string ret = "<" + defaultColor.ToHTMLString() + ">" + text.Substring(0, textColors[0].start);
        for (int i = 0; i < textColors.Count; i++)
        {
            ret += "<" + textColors[i].color.ToHTMLString() + ">";
            ret += text.Substring(textColors[i].start, textColors[i].end - textColors[i].start + 1);
            if (i == textColors.Count - 1)
            {
                break;
            }
            ret += "<" + defaultColor.ToHTMLString() + ">";
            ret += text.Substring(textColors[i].end + 1, textColors[i + 1].start - textColors[i].end - 1);
        }
        ret += "<" + defaultColor.ToHTMLString() + ">";
        ret += text.Substring(textColors[^1].end + 1);
        return ret;
    }

    public static implicit operator TextColored(string text)
    {
        return new TextColored(text, new SColor(1, 1, 1));
    }

    public static implicit operator string(TextColored text)
    {
        return text.text;
    }

    public override string ToString()
    {
        return text;
    }
}