using System.ComponentModel;

namespace GWAShared.Enums
{
    /// <summary>
    /// m_approval_flow.is_active, m_asset_category.is_active, m_checklist_template.is_active, m_controller.is_active, m_controller_action.is_active, m_division.is_active, m_document_category.is_active, m_document_number_format.is_active, m_dues_setting.is_active, m_external_party_contact.is_active, m_financial_account.is_active, m_financial_category.is_active, m_header_menu.is_active, m_letter_template.is_active, m_message_template.is_active, m_point_rule.is_active, m_position.is_active, m_product.is_active, m_product_variant.is_active, m_role.is_active, m_sponsorship_package.is_active, m_sub_menu.is_active, m_user.is_active, t_activity_schedule.is_active, t_banner.is_active, t_division_member.is_active
    /// </summary>
    public enum ActiveStatus
    {
        [Description("Tidak Aktif")] TidakAktif = 0,
        [Description("Aktif")] Aktif = 1
    }
}
