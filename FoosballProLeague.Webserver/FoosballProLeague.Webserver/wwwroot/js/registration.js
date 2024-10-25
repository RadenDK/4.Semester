$(document).ready(function() {
    $('#company').change(function() {
        var selectedCompanyId = $(this).val();
        $('#department').val('');
        $('#department option').each(function() {
            var departmentCompanyId = $(this).data('company-id');
            if (departmentCompanyId == selectedCompanyId || !selectedCompanyId) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
        $('#department').prop('disabled', !selectedCompanyId);
    });
    $('#company').change();

});