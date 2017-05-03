using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace WpfTreeDemo
{
    public sealed class MainWindowViewModel : Changeable
    {
        public MainWindowViewModel()
        {
            Root = Tree.Create("My Soccer Leagues",
                Tree.Create("League A",
                    Tree.Create("Division A", "Team 1", "Team 2", "Team 3"),
                    Tree.Create("Division B", new List<string> {"Team 4", "Team 5", "Team 6"}),
                    Tree.Create("Division C", new List<ITree<string>> {Tree.Create("Team 7"), Tree.Create("Team 8")})),
                Tree.Create("League B",
                    Tree.Create("Division A", new Tree<string>("Team 9"), new Tree<string>("Team 10"), new Tree<string>("Team 11")),
                    Tree.Create("Division B", Tree.Create("Team 12"), Tree.Create("Team 13"), Tree.Create("Team 14")),
                    Tree.Create("Division C", Tree.Create("Team 15"), Tree.Create("Team 16"), Tree.Create("Team 17"))));

            SelectedItem = Root;
        }

        private static int s_index;

        public ITree<string> Root { get; set; }

        private ITree<string> m_selectedItem;

        public static string DumpElement(ITree<string> element)
        {
            var sb = new StringBuilder();
            var offset = element.GetParents(false).Count();
            Tree.Visit(element, x =>
            {
                sb.Append(new string(' ', x.GetParents(false).Count() - offset));
                sb.AppendLine(x.ToString());
            });
            return sb.ToString();
        }

        public object SelectedItem
        {
            get { return m_selectedItem; }
            set
            {
                var item = value as ITree<string>;
                if (m_selectedItem != item)
                {
                    m_selectedItem = item;
                    ElementDump = DumpElement(m_selectedItem ?? Root);
                    NotifyOfPropertyChange(() => SelectedItem);
                }
            }
        }

        private string m_elementName;

        public string ElementName
        {
            get { return m_elementName; }
            set
            {
                if (m_elementName != value)
                {
                    m_elementName = value;
                    NotifyOfPropertyChange(() => ElementName);
                }
            }
        }

        private string m_elementDump;

        public string ElementDump
        {
            get { return m_elementDump; }
            set
            {
                if (m_elementDump != value)
                {
                    m_elementDump = value;
                    NotifyOfPropertyChange(() => ElementDump);
                }
            }
        }

        private ICommand m_addChildCommand;

        public ICommand AddChildCommand
        {
            get
            {
                return m_addChildCommand ?? (m_addChildCommand = new RelayCommand(() =>
                {
                    var item = m_selectedItem ?? Root;
                    var elementName = string.IsNullOrEmpty(ElementName) ? string.Format("Element {0}", ++s_index) : ElementName;
                    item.Add(Tree.Create(elementName));
                    ElementDump = DumpElement(item);
                }));
            }
        }

        private ICommand m_removeSelectedCommand;

        public ICommand RemoveSelectedCommand
        {
            get
            {
                return m_removeSelectedCommand ?? (m_removeSelectedCommand = new RelayCommand(
                    () => m_selectedItem.Parent.Items.Remove(m_selectedItem),
                    () => m_selectedItem != null && m_selectedItem.Parent != null));
            }
        }
    }
}