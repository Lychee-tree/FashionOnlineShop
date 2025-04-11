using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace nhom6.ViewModels
{


    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        public string userName { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải ít nhất 8 ký tự")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ in hoa và 1 chữ số")]
        [DataType(DataType.Password)]
        public string userPass { get; set; }

        [Required(ErrorMessage = "Nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("userPass", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string userEmail { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public int roleID { get; set; }

        // 🔽 List dropdown Role
        public List<SelectListItem> RoleList { get; set; }
    }
}

