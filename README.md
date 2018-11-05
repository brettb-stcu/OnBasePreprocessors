# OnBasePreprocessors
A collection of projects used to format input for various OnBase import methods.
## MemberNameChanges
Used by <i>AFKS - Member Info</i> to create <b>Member Name Add/Modify</b> Unity Forms which are processed by the <i>IM - Member Info Update</i> life cycle (LC). The LC updates the First Name and Last Name keywords on all related documents (Member No.) and stamps the Unity Form with an update date and a count of the related documents updated. Also triggers an Auto-Folder process to ensure that all supporting folders are created with the correct member name.
