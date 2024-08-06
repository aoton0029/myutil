using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Command
{
    public class Document
    {
        private StringBuilder _text = new StringBuilder();

        public void Insert(int position, string text)
        {
            _text.Insert(position, text);
        }

        public void Delete(int position, int length)
        {
            _text.Remove(position, length);
        }

        public string GetText(int position, int length)
        {
            return _text.ToString().Substring(position, Math.Min(length, _text.Length - position));
        }

        public override string ToString()
        {
            return _text.ToString();
        }
    }

    public class DeleteTextCommand : ICommand
    {
        private string _deletedText;
        private Document _document;
        private int _position; // Position from which text was deleted

        public DeleteTextCommand(Document document, int position, int length)
        {
            _document = document;
            _position = position;
            _deletedText = _document.GetText(position, length);
        }

        public void Execute()
        {
            _document.Delete(_position, _deletedText.Length);
        }

        public void Undo()
        {
            _document.Insert(_position, _deletedText);
        }
    }

    public class InsertTextCommand : ICommand
    {
        private string _textToInsert;
        private Document _document;
        private int _position; // Position at which text is inserted

        public InsertTextCommand(Document document, string text, int position)
        {
            _document = document;
            _textToInsert = text;
            _position = position;
        }

        public void Execute()
        {
            _document.Insert(_position, _textToInsert);
        }

        public void Undo()
        {
            _document.Delete(_position, _textToInsert.Length);
        }
    }
}
