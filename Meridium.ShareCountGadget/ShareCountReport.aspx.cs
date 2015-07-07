using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using EPiServer;
using EPiServer.Configuration;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Filters;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;
using EPiServer.Shell.WebForms;
using EPiServer.Web.PageExtensions;

namespace Meridium.ShareCountGadget
{
    [GuiPlugIn(
    Area = PlugInArea.ReportMenu,
    DisplayName = "Delningar i sociala medier",
    Description = "Visar totalt antal delningar i sociala medier",
    Category = "Custom Reports",
    Url = "~/Reports/ShareCountReport.aspx")]
    public partial class ShareCountReport : WebFormsBase
    {
        /* *******************************************************************
        *  Properties
        * *******************************************************************/
        private int _rowCount;

        public PageReference RootPage
        {
            get
            {
                return (PageReference)(ViewState["RootPage"] ?? ContentReference.StartPage);
            }
            set
            {
                ViewState["RootPage"] = value;
            }
        }
        public DateTime StartDate
        {
            get
            {
                return (DateTime)(ViewState["StartDate"] ?? DateTime.MinValue);
            }
            set
            {
                ViewState["StartDate"] = value;
            }
        }
        public int LanguageId
        {
            get
            {
                return (int)(ViewState["LanguageId"] ?? -1);
            }
            set
            {
                ViewState["LanguageId"] = value;
            }
        }
        public bool IsChangedByCurrentUser
        {
            get
            {
                return (bool)(ViewState["IsChangedByCurrentUser"] ?? false);
            }
            set
            {
                ViewState["IsChangedByCurrentUser"] = value;
            }
        }
        public DateTime EndDate
        {
            get
            {
                return (DateTime)(ViewState["EndDate"] ?? DateTime.MinValue);
            }
            set
            {
                ViewState["EndDate"] = value;
            }
        }

        /* *******************************************************************
        *  Methods
        * *******************************************************************/
        public IEnumerable<PageData> GetPages(PageReference startPage, int languageId, bool isChangedByCurrentUser, DateTime startDate, DateTime stopDate, string sortExpression, int maximumRows, int startRowIndex, out int rowCount)
        {
            var langRepository = ServiceLocator.Current.GetInstance<ILanguageBranchRepository>();
            var languages = languageId == -1
                ? langRepository.ListAll()
                : new List<LanguageBranch> { langRepository.Load(languageId) };

            var repo = ServiceLocator.Current.GetInstance<IContentRepository>();

            var refs = repo.GetDescendents(startPage);

            List<PageData> collPages = new List<PageData>();
            foreach (var branch in languages)
            {                
                var lang = new LanguageSelector(branch.LanguageID);
                var langPages = repo.GetItems(refs, lang).OfType<PageData>().ToList();
                collPages.AddRange(langPages);
            }

            var filteredPages = from page in collPages
                                where page is ISocialMediaTrackable
                                where (page.StartPublish >= startDate && page.StartPublish <= stopDate)
                                select page;

            if (isChangedByCurrentUser)
            {
                filteredPages = from page in filteredPages
                    where page.ChangedBy == HttpContext.Current.User.Identity.Name
                    select page;
            }

            new FilterPublished(PagePublishedStatus.Published).Filter(new PageDataCollection(filteredPages));
            rowCount = filteredPages.ToList().Count;
            _rowCount = rowCount;
            return Order(isChangedByCurrentUser ? filteredPages.Where(p => p.ChangedBy.Equals(User.Identity.Name)) : filteredPages, sortExpression).Skip(startRowIndex).Take(maximumRows);
        }

        public int GetRowCount(PageReference startPage, int languageId, bool isChangedByCurrentUser, DateTime startDate, DateTime stopDate, out int rowCount)
        {
            rowCount = _rowCount;
            return _rowCount;
        }

        private SortDirection GetSortDirection(string sortExpression)
        {
            var direction = SortDirection.Ascending;
            if (string.IsNullOrEmpty(sortExpression)) return direction;
            var strArray = sortExpression.Split(new[] { ' ' });
            if ((strArray.Length == 2) && string.Equals(strArray[1], "DESC", StringComparison.OrdinalIgnoreCase))
                direction = SortDirection.Descending;
            return direction;
        }

        public IEnumerable<PageData> Order(IEnumerable<PageData> pages, string sortExpression)
        {
            var sortdirection = GetSortDirection(sortExpression);
            switch (sortExpression.Split(new[] { ' ' }).ElementAt(0))
            {
                case "PageName":
                    return sortdirection == SortDirection.Ascending
                        ? pages.OrderBy(p => p.PageName)
                        : pages.OrderByDescending(p => p.PageName);
                case "TotalShares":
                    return sortdirection == SortDirection.Ascending
                        ? pages.OrderBy(p => ((ISocialMediaTrackable)p).ShareCount.TotalShareCount)
                        : pages.OrderByDescending(p => ((ISocialMediaTrackable)p).ShareCount.TotalShareCount);
                case "Facebook":
                    return sortdirection == SortDirection.Ascending
                        ? pages.OrderBy(p => int.Parse(((ISocialMediaTrackable)p).ShareCount.FacebookShareCount))
                        : pages.OrderByDescending(p => int.Parse(((ISocialMediaTrackable)p).ShareCount.FacebookShareCount));
                case "Twitter":
                    return sortdirection == SortDirection.Ascending
                        ? pages.OrderBy(p => int.Parse(((ISocialMediaTrackable)p).ShareCount.TwitterShareCount))
                        : pages.OrderByDescending(p => int.Parse(((ISocialMediaTrackable)p).ShareCount.TwitterShareCount));
            }
            return pages;
        }

        /* *******************************************************************
        *  Event methods
        * *******************************************************************/
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            base.SystemMessageContainer.Heading = Translate("/reportcenter/report[@name='changedpages']/heading");
            base.SystemMessageContainer.Description = Translate("/reportcenter/report[@name='changedpages']/info");
            base.SystemMessageContainer.HeadingImageUrl = ThemeUtility.GetImageThemeUrl(Page, "ReportCenter/ChangedPages.gif");
            ReportView.PageSize = int.Parse(this.PageSizeSelector.SelectedValue);
            if (!IsPostBack)
            {

                StartPageSelector.PageLink = this.RootPage;
                SetTime_Click(null, new CommandEventArgs("LastWeek", null));

                if (Settings.Instance.UIShowGlobalizationUserInterface)
                {
                    var languagelocator = ServiceLocator.Current.GetInstance<LanguageBranchRepository>();
                    this.LanguageSelector.DataSource = languagelocator.ListEnabled();
                    this.LanguageSelector.DataBind();
                }
                else
                {
                    LanguageSelectionContainer.Visible = false;
                    foreach (DataControlField field in ReportView.Columns)
                    {
                        if ((field is BoundField) && string.Equals(((BoundField)field).DataField, "LanguageId", StringComparison.OrdinalIgnoreCase))
                        {
                            field.Visible = false;
                        }
                    }
                }
            }
        }
        protected void SetTimeClick(object sender, CommandEventArgs e)
        {
            DateTime time = DateTime.Today.AddDays(1.0);
            string commandName = e.CommandName;
            if (commandName != null)
            {
                if (commandName != "Today")
                {
                    if (commandName != "Yesterday")
                    {
                        if (commandName != "LastWeek")
                        {
                            if (commandName == "LastMonth")
                            {
                                StartDateSelector.Value = time.AddDays(-30.0);
                                EndDateSelector.Value = time;
                            }
                            return;
                        }
                        StartDateSelector.Value = time.AddDays(-7.0);
                        EndDateSelector.Value = time;
                        return;
                    }
                }
                else
                {
                    StartDateSelector.Value = DateTime.Today;
                    EndDateSelector.Value = time;
                    return;
                }
                StartDateSelector.Value = DateTime.Today.AddDays(-1.0);
                EndDateSelector.Value = DateTime.Today;
            }
        }
        protected void TimePeriodChanged(object sender, EventArgs e)
        {
            string selectedValue = TimePeriod.SelectedValue;
            if (selectedValue != null)
            {
                if (selectedValue != "Between")
                {
                    if (selectedValue != "Before")
                    {
                        if (selectedValue == "After")
                        {
                            Quicklinks.Visible = false;
                            StartDateSelector.Visible = true;
                            EndDateSelector.Visible = false;
                            DateDash.Visible = false;
                            StartDateSelector.Value = DateTime.Today.AddDays(-6.0);
                            EndDateSelector.Value = PropertyDate.MaxValue;
                        }
                        return;
                    }
                }
                else
                {
                    Quicklinks.Visible = true;
                    StartDateSelector.Visible = true;
                    EndDateSelector.Visible = true;
                    DateDash.Visible = true;
                    SetTime_Click(null, new CommandEventArgs("LastWeek", null));
                    return;
                }
                Quicklinks.Visible = false;
                StartDateSelector.Visible = false;
                EndDateSelector.Visible = true;
                DateDash.Visible = false;
                StartDateSelector.Value = PropertyDate.MinValue;
                EndDateSelector.Value = DateTime.Today.AddDays(1.0);
            }
        }
        protected void SetTime_Click(object sender, CommandEventArgs e)
        {
            DateTime time = DateTime.Today.AddDays(1.0);
            string commandName = e.CommandName;
            if (commandName != null)
            {
                if (commandName != "Today")
                {
                    if (commandName != "Yesterday")
                    {
                        if (commandName != "LastWeek")
                        {
                            if (commandName == "LastMonth")
                            {
                                StartDateSelector.Value = time.AddDays(-30.0);
                                EndDateSelector.Value = time;
                            }
                            return;
                        }
                        StartDateSelector.Value = time.AddDays(-7.0);
                        EndDateSelector.Value = time;
                        return;
                    }
                }
                else
                {
                    StartDateSelector.Value = DateTime.Today;
                    EndDateSelector.Value = time;
                    return;
                }
                StartDateSelector.Value = DateTime.Today.AddDays(-1.0);
                EndDateSelector.Value = DateTime.Today;
            }
        }
        protected void ShowReportClick(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                RootPage = StartPageSelector.PageLink;
                IsChangedByCurrentUser = ChangedByCurrentUser.Checked;
                int result = 10;
                int.TryParse(PageSizeSelector.SelectedValue, out result);
                ReportView.PageSize = result;
                ReportView.PageIndex = 0;
                int num2 = -1;
                int.TryParse(this.LanguageSelector.SelectedValue, out num2);
                LanguageId = num2;
                StartDate = StartDateSelector.Value;
                EndDate = EndDateSelector.Value;
                ReportView.DataSourceID = ChangedReport.ID;
                ReportView.DataBind();
            }
        }
        protected void ReportDataObjectCreating(object sender, ObjectDataSourceEventArgs e)
        {
            var instance = ServiceLocator.Current.GetInstance<ShareCountReport>();
            e.ObjectInstance = instance;
        }
        protected void ReportDataSelected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            HitsCount.Text = string.Format(Translate("/reportcenter/numberofhits"), e.OutputParameters["rowCount"]);
        }
    }
}