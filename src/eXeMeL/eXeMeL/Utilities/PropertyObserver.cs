using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace eXeMeL.Utilities
{
  /// <summary>
  /// In-tree replacement for MvvmFoundation.Wpf.PropertyObserver.
  /// Monitors an INotifyPropertyChanged source and fires registered
  /// handlers when the specified property changes.
  /// </summary>
  public class PropertyObserver<T> where T : INotifyPropertyChanged
  {
    private readonly T _source;
    private readonly Dictionary<string, Action<T>> _handlers = new Dictionary<string, Action<T>>();

    public PropertyObserver(T source)
    {
      _source = source ?? throw new ArgumentNullException(nameof(source));
      _source.PropertyChanged += SourcePropertyChanged;
    }

    public PropertyObserver<T> RegisterHandler(Expression<Func<T, object>> expression, Action<T> handler)
    {
      var propertyName = GetPropertyName(expression);
      _handlers[propertyName] = handler;
      return this;
    }

    private void SourcePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName != null && _handlers.TryGetValue(e.PropertyName, out var handler))
      {
        handler(_source);
      }
    }

    private static string GetPropertyName(Expression<Func<T, object>> expression)
    {
      var body = expression.Body;

      // Handle boxing conversion for value types (e.g., s => (object)s.SomeValueType)
      if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
      {
        body = unary.Operand;
      }

      if (body is MemberExpression member)
      {
        return member.Member.Name;
      }

      throw new ArgumentException("Expression must be a property access expression.", nameof(expression));
    }
  }
}
