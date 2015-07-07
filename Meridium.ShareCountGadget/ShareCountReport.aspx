<%@ Page Language="C#" AutoEventWireup="false" CodeBehind="ShareCountReport.aspx.cs" Inherits="Meridium.ShareCountGadget.ShareCountReport" EnableViewState="true" %>
<%@ Import Namespace="EPiServer" %>
<%@ Import Namespace="EPiServer.Editor" %>
<%@ Register TagPrefix="EPiServerUI" Namespace="EPiServer.UI.WebControls" Assembly="EPiServer.UI" %>
<asp:Content ContentPlaceHolderID="FullRegion" Runat="Server">
<div class="epi-contentContainer epi-padding">
    
        <div>
	        <h1 class="EP-prefix">
		        Delade sidor
	        </h1>
            <p class="EP-systemInfo">
              Den här rapporten visar sidor som delats i sociala medier.
              Välj en sida i resultatlistan för att öppna den i redigeraläget.
            </p>
        </div>


        <div class="epi-formArea">
            <fieldset>
                <legend>
                    <asp:Literal Text="<%$ Resources: EPiServer, reportcenter.reportcriterias.heading %>" runat="server"/>
                </legend>
                 <div class="epi-size10">
                     
                    <div>
                        <asp:label runat="server" Text="<%$ Resources: EPiServer, reportcenter.reportcriterias.published %>" AssociatedControlID="TimePeriod" />
                        <asp:Panel ID="Quicklinks" CssClass="epilinklist" style="display:inline;" runat="server">
                            <asp:LinkButton OnCommand="SetTimeClick" CommandName="LastMonth" Text="<%$ Resources: EPiServer, button.lastmonth %>" runat="server" />
                            <asp:LinkButton OnCommand="SetTimeClick" CommandName="LastWeek" Text="<%$ Resources: EPiServer, button.lastweek %>" runat="server" />
                            <asp:LinkButton OnCommand="SetTimeClick" CommandName="Yesterday" Text="<%$ Resources: EPiServer, button.yesterday %>" runat="server" />
                            <asp:LinkButton OnCommand="SetTimeClick" CommandName="Today" Text="<%$ Resources: EPiServer, button.today %>" runat="server" />
                        </asp:Panel>
                    </div>
                    <div class="epi-indent">
                        <asp:DropDownList SkinId="Custom" runat="server" ID="TimePeriod" AutoPostBack="true" OnSelectedIndexChanged="TimePeriodChanged">
                            <asp:ListItem Value="Before" Text="<%$ Resources: EPiServer, reportcenter.reportcriterias.before %>" />
                            <asp:ListItem Selected="True" Value="Between" Text="<%$ Resources: EPiServer, reportcenter.reportcriterias.between %>" />
                            <asp:ListItem Value="After" Text="<%$ Resources: EPiServer, reportcenter.reportcriterias.after %>" />
                        </asp:DropDownList>
                        <EPiServer:InputDate style="width: auto;" CssClass="epiinlineinputcontrol" ID="StartDateSelector" runat="server" /> 
                        <asp:Literal runat="server" ID="DateDash" Text="─" /> 
                        <EPiServer:InputDate style="width: auto;" CssClass="epiinlineinputcontrol" ID="EndDateSelector" runat="server" />
                    </div>
                    <div>
                        <asp:label runat="server" Text="<%$ Resources: EPiServer, reportcenter.reportcriterias.selectrootpage %>" AssociatedControlID="StartPageSelector" />
                        <EPiServer:InputPageReference CssClass="epiinlineinputcontrol" ID="StartPageSelector" runat="server" DisableCurrentPageOption="true"/>
                    </div>
                     <div>
                        <asp:Label runat="server" Text="<%$ Resources: EPiServer, reportcenter.reportcriterias.changedbyme %>" AssociatedControlID="ChangedByCurrentUser" />
                        <asp:CheckBox runat="server" ID="ChangedByCurrentUser" Checked="false" />
                    </div>
                    <div ID="LanguageSelectionContainer" runat="server">
                        <asp:label runat="server" Text="<%$ Resources: EPiServer, reportcenter.reportcriterias.selectlanguage %>" AssociatedControlID="LanguageSelector" />
                        <asp:DropDownList runat="server" ID="LanguageSelector" AppendDataBoundItems="true" DataTextField="Name" DataValueField="ID">
                            <asp:ListItem Text="<%$ Resources: EPiServer, reportcenter.reportcriterias.all %>" Value="-1" Selected="True" />
                        </asp:DropDownList>
                    </div>
                </div>
            </fieldset>
            <div class="epitoolbuttonrow">
                <EPiServerUI:ToolButton runat="server" SkinID="Report" Text="<%$ Resources: EPiServer, button.showreport %>" OnClick="ShowReportClick" />
            </div>
            
            <div class="epi-floatRight epi-marginVertical-small">
                <asp:label runat="server" Text="<%$ Resources: EPiServer, reportcenter.reportcriterias.numberofitemsperpage %>" AssociatedControlID="PageSizeSelector" />
                <asp:DropDownList runat="server" SkinId="Custom" ID="PageSizeSelector">
                    <asp:ListItem Text="25" Value="25" Selected="True" />
                    <asp:ListItem Text="50" Value="50" />
                    <asp:ListItem Text="100" Value="100" />
                </asp:DropDownList>
            </div>
            <div class="epi-floatLeft epi-marginVertical-small">
                <asp:Literal ID="HitsCount" runat="server" />
            </div>
            <div class="epi-contentArea epi-clear">
                <EPiServerUI:SortedGridView ID="ReportView" runat="server" 
                    AutoGenerateColumns="false"
                    DefaultSortDirection="Descending" DefaultSortExpression="TotalShares"
                    Width="100%">
                    <Columns>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, reportcenter.reportcolumnheadings.pagename %>" ItemStyle-Width="27%" SortExpression="PageName">
                            <ItemTemplate >
                                <a href="<%# UriSupport.ResolveUrlFromUIBySettings(PageEditing.GetEditUrl(((EPiServer.Core.PageData)Container.DataItem).PageLink)) %>"><%#((EPiServer.Core.PageData)Container.DataItem).PageName %></a>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Totalt antal delningar" SortExpression="TotalShares">
                            <ItemTemplate>                                
                                <%# Eval("ShareCount.TotalShareCount") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Delningar på Facebook" SortExpression="Facebook">
                            <ItemTemplate>
                                <%# Eval("ShareCount.FacebookShareCount") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Delningar på Twitter" SortExpression="Twitter">
                            <ItemTemplate> 
                                <%# Eval("ShareCount.TwitterShareCount") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerSettings Mode="NumericFirstLast" />
                </EPiServerUI:SortedGridView>
            </div>
        </div>
        <asp:ObjectDataSource ID="ChangedReport" runat="server" SortParameterName="sortExpression"
                TypeName="Meridium.ShareCountGadget.ShareCountReport" 
                SelectMethod="GetPages" SelectCountMethod="GetRowCount" EnablePaging="true" 
                OnSelected="ReportDataSelected" OnObjectCreating="ReportDataObjectCreating">
            <SelectParameters>
                <asp:ControlParameter ControlID="__Page" Direction="Input" Type="Object" PropertyName="RootPage" Name="startPage" />
                <asp:ControlParameter ControlID="__Page" Direction="Input" Type="Int32" PropertyName="LanguageId" Name="languageId" />
                <asp:ControlParameter ControlID="__Page" Direction="Input" Type="Boolean" PropertyName="IsChangedByCurrentUser" Name="isChangedByCurrentUser" />
                <asp:ControlParameter ControlID="__Page" Direction="Input" PropertyName="StartDate" Name="startDate" />
                <asp:ControlParameter ControlID="__Page" Direction="Input" PropertyName="EndDate" Name="stopDate" />
                <asp:Parameter Direction="Output" Name="rowCount" DefaultValue="0"/>
            </SelectParameters>
        </asp:ObjectDataSource>
    </div>    
</asp:Content>