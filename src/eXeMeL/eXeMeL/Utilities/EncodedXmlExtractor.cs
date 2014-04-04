using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace eXeMeL.Utilities
{
  class EncodedXmlExtractor
  {
    public string Text { get; private set; }



    public EncodedXmlExtractor(string text)
    {
      this.Text = text;
    }



    public async Task<string> GetDecodedXmlAroundIndexAsync(int caretOffset)
    {
      string decodedText = null;

      await Task.Run(() =>
      {
        if (caretOffset >= this.Text.Length)
          return;

        var isCaretInTextElement = IsCaretInTextElement(caretOffset);
        var isCaretInAttribute = IsCaretInAttribute(caretOffset);
        if (!isCaretInTextElement && !isCaretInAttribute)
        {
          //this.MessengerInstance.Send(new DisplayApplicationStatusMessage("Unable to copy.  Text was not an attribute or a text element."));
          return;
        }


        var startingCharacterToLookFor = '"';
        var endingCharacterToLookFor = '"';
        if (isCaretInTextElement)
        {
          startingCharacterToLookFor = '>';
          endingCharacterToLookFor = '<';
        }


        int startOffset = GetIndexOfFirstCharacterOfBlock(caretOffset, startingCharacterToLookFor);
        if (startOffset <= 0)
          return;

        int endOffset = GetIndexOfLastCharacterInBlock(caretOffset, endingCharacterToLookFor);
        if (endOffset >= this.Text.Length)
          return;

        var enclosedText = this.Text.Substring(startOffset, endOffset - startOffset); //this.Document.GetText(startOffset, endOffset - startOffset);

        decodedText = HttpUtility.HtmlDecode(enclosedText);
      });

      return decodedText;
    }



    private bool IsCaretInTextElement(int caretOffset)
    {
      while (caretOffset > 0)
      {
        var currentCharacter = this.Text[caretOffset];

        if (currentCharacter == '<')
        {
          return false;
        }
        else if (currentCharacter == '>')
        {
          return true;
        }

        caretOffset -= 1;
      }

      return false;
    }



    private bool IsCaretInAttribute(int caretOffset)
    {
      while (caretOffset > 0)
      {
        var currentCharacter = this.Text[caretOffset];

        if (currentCharacter == '"')
        {
          return true;
        }
        else if (currentCharacter == '<')
        {
          return false;
        }
        else if (currentCharacter == '>')
        {
          return false;
        }

        caretOffset -= 1;
      }

      return false;
    }



    private int GetIndexOfFirstCharacterOfBlock(int caretOffset, char startingCharacterToLookFor)
    {
      var startOffset = caretOffset;
      while (startOffset > 0)
      {
        var currentCharacter = this.Text[startOffset];

        if (currentCharacter == startingCharacterToLookFor)
        {
          startOffset += 1;
          break;
        }

        startOffset -= 1;
      }
      return startOffset;
    }



    private int GetIndexOfLastCharacterInBlock(int caretOffset, char endingCharacterToLookFor)
    {
      var endOffset = caretOffset;

      while (endOffset <= this.Text.Length)
      {
        var currentCharacter = this.Text[endOffset];

        if (currentCharacter == endingCharacterToLookFor)
          break;

        endOffset += 1;
      }
      return endOffset;
    }


  }
}
