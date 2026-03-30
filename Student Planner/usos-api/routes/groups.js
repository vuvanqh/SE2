const express = require('express');
const router = express.Router();

const { getUserGroups, getGroupById, getGroupParticipants } = require('../data/groupsData'); 

// GET /services/groups/user
router.get('/user', (req, res) => {
    const { user_id } = req.query;

    if (!user_id) return res.status(400).json({ error: 'user_id is required' });

    const groups = getUserGroups(user_id);
    res.json({ 
        user_id, 
        groups,
        terms: Object.keys(groups) 
    });
});

// GET /services/groups/group 
router.get('/group', (req, res) => {
  const { course_id, group_number, class_type, term_id } = req.query;

  if (!course_id || !group_number || !class_type || !term_id) {
    return res.status(400).json({ error: 'missing params' });}

  const group = getGroupById(course_id, group_number, class_type, term_id);
  
  if (!group) {
    return res.status(404).json({ error: 'Group not found' });}

  res.json(group);
});

// GET /services/groups/is-participant
router.get('/is-participant', (req, res) => {
    const { user_id, course_id, group_number, class_type, term_id } = req.query;
    const participants = getGroupParticipants(course_id, group_number, class_type, term_id);
    const isParticipant = participants.some(p => p.student_id === user_id);
    res.json(isParticipant);
});

// GET /services/groups/participants/:course_id/:group_number/:class_type/:term_id
// Example: /services/groups/participants?course_id=100&group_number=1&class_type=Lecture&term_id=2025Z
router.get('/participants', (req, res) => {
    const { course_id, group_number, class_type, term_id } = req.query;

    if (!course_id || !group_number || !class_type || !term_id) {
      return res.status(400).json({ error: 'missing params' });
    }
    const participants = getGroupParticipants(course_id, group_number, class_type, term_id);

    res.json(participants);
});

module.exports = router;