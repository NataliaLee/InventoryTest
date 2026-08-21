using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.InventoryLogic.Items
{
    public sealed class ContainerItem : Item, IContainer
    {
        private readonly ContainerContent[] _contents;
        public IReadOnlyList<ContainerContent> Contents => _contents;

        public ContainerItem(string id, IReadOnlyCollection<ContainerContent> contents) : base(id)
        {
            if (contents == null)
                throw new ArgumentNullException(nameof(contents));

            _contents = new ContainerContent[contents.Count];

            var index = 0;

            foreach (var content in contents)
                _contents[index++] = content;
        }

    }
}
