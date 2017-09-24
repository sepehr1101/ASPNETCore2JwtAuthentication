using System.Collections.Generic;
namespace AuthServer.DomainClasses.ViewModels
{    
    public class TreeItem<T>
    {
        public T Item { get; set; }
        public IEnumerable<TreeItem<T>> Children { get; set; }
    }
    
}