﻿using System;
using System.Text;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class order_goods_list : System.Web.UI.Page
{
    protected int totalCount;
    protected int page;
    protected int pageSize;

    protected int product_category_id;
    protected int depot_category_id=0;
    protected string note_no = string.Empty;
    protected int product_series_id;

    ManagePage mym = new ManagePage();
    protected void Page_Load(object sender, EventArgs e)
    {
        //判断是否登录
        if (!mym.IsAdminLogin())
        {
            Response.Write("<script>parent.location.href='../index.aspx'</script>");
            Response.End();
        }
        //判断权限
        ps_manager_role_value myrv = new ps_manager_role_value();
        int role_id = Convert.ToInt32(Session["RoleID"]);
        depot_category_id = Convert.ToInt32(Session["DepotCatID"]);

        int nav_id =21;
        if (!myrv.QXExists(role_id, nav_id))
        {
            Response.Redirect("../error.html");
            Response.End();
        }
        this.product_category_id = AXRequest.GetQueryInt("product_category_id");
        this.note_no = AXRequest.GetQueryString("note_no");
        this.product_series_id = AXRequest.GetQueryInt("product_series_id");

        this.pageSize = GetPageSize(20); //每页数量

        if (!Page.IsPostBack)
        {
            ZYBind();//绑定商品类别
            PSBind(); //绑定产品系列
            //RptBind("id>0" + CombSqlTxt(this.product_category_id, this.note_no, this.depot_category_id), "add_time desc,id desc");
            RptBind("id>0" + CombSqlTxt(this.product_category_id, this.note_no, this.depot_category_id, this.product_series_id), "is_kit desc, id desc");
        }
    }


    #region 绑定商品类别=================================
    private void ZYBind()
    {
        ps_product_category bll = new ps_product_category();
        DataTable dt = bll.GetList("1=1").Tables[0];
        this.ddlproduct_category_id.Items.Clear();
        this.ddlproduct_category_id.Items.Add(new ListItem("==全部==", "0"));
        foreach (DataRow dr in dt.Rows)
        {
            string Id = dr["id"].ToString();
            string Title = dr["title"].ToString().Trim();
            this.ddlproduct_category_id.Items.Add(new ListItem(Title, Id));
        }
    }
    #endregion
    #region 绑定产品系列=================================
    private void PSBind()
    {
        ps_product_series bll = new ps_product_series();
        DataTable dt = bll.GetList("1=1").Tables[0];
        this.ddlproduct_series_id.Items.Clear();
        this.ddlproduct_series_id.Items.Add(new ListItem("==全部==", "0"));
        foreach (DataRow dr in dt.Rows)
        {
            string Id = dr["id"].ToString();
            string Title = dr["name"].ToString().Trim();
            this.ddlproduct_series_id.Items.Add(new ListItem(Title, Id));
        }
    }
    #endregion

    #region 数据绑定=================================
    private void RptBind(string _strWhere, string _orderby)
    {
        this.page = AXRequest.GetQueryInt("page", 1);

        if (this.product_category_id > 0)
        {
            this.ddlproduct_category_id.SelectedValue = this.product_category_id.ToString();
        }
        txtNote_no.Text = this.note_no;
        if (this.product_series_id > 0)
        {
            this.ddlproduct_series_id.SelectedValue = this.product_series_id.ToString();
        }

        ps_here_depot bll = new ps_here_depot();
        this.rptList.DataSource = bll.GetList(this.pageSize, this.page, _strWhere, _orderby, out this.totalCount);
        this.rptList.DataBind();

        //绑定页码
        txtPageNum.Text = this.pageSize.ToString();
        string pageUrl = Utils.CombUrlTxt("goods_list.aspx", "product_category_id={0}&note_no={1}&product_series_id={2}&page={3}", this.product_category_id.ToString(), this.note_no, this.ddlproduct_series_id.SelectedValue, "__id__");
        PageContent.InnerHtml = Utils.OutPageList(this.pageSize, this.page, this.totalCount, pageUrl, 8);
    }
    #endregion

    #region 组合SQL查询语句==========================
    protected string CombSqlTxt(int _product_category_id, string _note_no,int _depot_category_id, int _product_series_id)
    {
        StringBuilder strTemp = new StringBuilder();

        strTemp.Append(" and is_xs=0");
        strTemp.Append(" and status=0");
        if (_product_category_id > 0)
        {
            strTemp.Append(" and product_category_id=" + _product_category_id);
        }

        if (_depot_category_id > 0)
        {
            strTemp.Append(" and company= isnull((select top 1 title from  ps_depot_category where id= '" + _depot_category_id + "'),'') ");
        }

        _note_no = _note_no.Replace("'", "");
        if (!string.IsNullOrEmpty(_note_no)) 
        {
            strTemp.Append(" and (product_name like  '%" + _note_no + "%' or product_no like  '%" + _note_no + "%' or specification like  '%" + _note_no + "%' or commercialStyle like  '%" + _note_no + "%') ");
        }

        if (_product_series_id > 0)
        {
            strTemp.Append(" and product_series_id=" + _product_series_id);
        }
        return strTemp.ToString();
    }
    #endregion

    #region 返回每页数量=============================
    private int GetPageSize(int _default_size)
    {
        int _pagesize;
        if (int.TryParse(Utils.GetCookie("goods_list_page_size"), out _pagesize))
        {
            if (_pagesize > 0)
            {
                return _pagesize;
            }
        }
        return _default_size;
    }
    #endregion


    //查询
    protected void btnSearch_Click(object sender, EventArgs e)
    {
        Response.Redirect(Utils.CombUrlTxt("goods_list.aspx", "product_category_id={0}&note_no={1}&product_series_id={2}", this.ddlproduct_category_id.SelectedValue, txtNote_no.Text, this.ddlproduct_series_id.SelectedValue));
    }


    //筛选商品类别
    protected void ddlproduct_category_id_SelectedIndexChanged(object sender, EventArgs e)
    {
        Response.Redirect(Utils.CombUrlTxt("goods_list.aspx", "product_category_id={0}&note_no={1}&product_series_id={2}", this.ddlproduct_category_id.SelectedValue, txtNote_no.Text, this.ddlproduct_series_id.SelectedValue));
    }

    protected void ddlproduct_series_id_SelectedIndexChanged(object sender, EventArgs e)
    {
        Response.Redirect(Utils.CombUrlTxt("goods_list.aspx", "product_category_id={0}&note_no={1}&product_series_id={2}", this.ddlproduct_category_id.SelectedValue, txtNote_no.Text, this.ddlproduct_series_id.SelectedValue));
    }

    

    //设置分页数量
    protected void txtPageNum_TextChanged(object sender, EventArgs e)
    {
        int _pagesize;
        if (int.TryParse(txtPageNum.Text.Trim(), out _pagesize))
        {
            if (_pagesize > 0)
            {
                Utils.WriteCookie("goods_list_page_size", _pagesize.ToString(), 14400);
            }
        }
        Response.Redirect(Utils.CombUrlTxt("goods_list.aspx", "product_category_id={0}&note_no={1}&product_series_id={2}", this.ddlproduct_category_id.SelectedValue, txtNote_no.Text, this.ddlproduct_series_id.SelectedValue));
    }

    //小数位是0的不显示
    public string MyConvert(object d)
    {
        string myNum = d.ToString();
        string[] strs = d.ToString().Split('.');
        if (strs.Length > 1)
        {
            if (Convert.ToInt32(strs[1]) == 0)
            {
                myNum = strs[0];
            }
        }
        return myNum;
    }

    //负数红色显示
    public string MyZF(object d)
    {
        string myNum = d.ToString();
        if (Convert.ToInt32(d.ToString()) <= 0)
        {
            myNum = "<font color=red> " + d.ToString() + "</font>";
        }
        return myNum;
    }
}