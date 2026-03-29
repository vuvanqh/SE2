const express = require('express');
const router = express.Router();

// const faculties = [
//   { faculty_id: "MINI", faculty_name: "Mathematics and Information Science", faculty_code: "MINI01" },
//   { faculty_id: "WEiTI", faculty_name: "Electronics and IT", faculty_code: "WEiTI01" }
// ];

const {getFaculties} = require('../data/facultiesData');

// GET /services/faculties
router.get('/', (req, res) => {
  res.json(getFaculties());
});

module.exports = router;