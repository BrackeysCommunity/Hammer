# Context Menus & Reactions

Hammer makes use of modern Discord API features such as slash commands and context menus. Below is a list of the available context
menus. Each context menu is also available as a message react which serves as a failsafe in the event that a context menu action
fails to register or execute.

### ✉️ Message > Apps > Report Message

**Public action.** Allows members to report a message which may be rule-breaking. This prevents members spam-pinging the staff
team.
A report is sent to the log channel, and an `@here` ping is used. If more reports are received than the threshold defined in the
config, `@everyone` is pinged. Only one report per-user-per-message is acknowledged. The same user reporting the same message will
have no effect.

The default reaction equivalent is the 🚩 emoji (`:triangular_flag_on_post:`). The config key
is `GUILD_ID.reactions.reportReaction`.

### ✉️ Message > Apps > Delete Message

**Staff action.** Removes a message, and sends a private message to the author notifying them of the deletion, indicating that
their message violated server rules.

The default reaction equivalent is the 🗑️ emoji (`:wastebasket:`). The config key is `GUILD_ID.reactions.deleteMessageReaction`.

### 👤 User > Apps > Gag

**Staff action.** Gags a user. This places a configurable timeout on the user (by default, this timeout is 5 minutes), allowing
staff to write a more formal and concrete infraction (warning, mute, ban, etc.)

The default reaction equivalent is the 🔇️emoji (`:mute:`). The config key is `GUILD_ID.reactions.gagReaction`.

### 👤 User > Apps > View Infraction History

**Staff action.** Views the infraction history of a user. This is identical to the `/history` command, except that the response **is ephemeral**.

The default reaction equivalent is the 🕓 emoji (`:clock4:`). The config key is `GUILD_ID.reactions.historyReaction`.k4: ). When
this reaction is used, the history is instead sent as a DM to the staff member.

### 👤 User > Apps > User Information

**Public action.** Displays information about a user. This is identical to the `/userinfo` command, except that the response **is ephemeral**.

There is no reaction equivalent for this action.

# Slash Commands

Below is an outline of every slash command currently implemented in Hammer, along with their descriptions and parameters.

## v3 Migration

Upon first startup, it is recommended to perform migration of the v3 infraction database using the `/migrate` command.
This command starts a wizard which will walk you through the process of upgrading a v3 users.json file to the v4 database.

Infraction IDs are maintained where possible, but in the event that an ID is occupied, a new ID is generated.
For this reason, the best results come from performing migration on initial setup, and only once.

## Message Report Blocking

As mentioned above, users have the ability to report messages. However, this opens up the potential to be abused. If a user is
sending too many frivolous reports, their reports can be blocked so that their reports are no longer acknowledged.

### `/blockreports`

Prevent a user's message reports from being acknowledged.

| Parameter | Required | Type               | Description                      |
|:----------|:---------|:-------------------|:---------------------------------|
| user      | ✅ Yes    | User mention or ID | The user whose reports to block. |

### `/unblockreports`

Unblocks a user, so that their message reports are acknowledged.

| Parameter | Required | Type               | Description                        |
|:----------|:---------|:-------------------|:-----------------------------------|
| user      | ✅ Yes    | User mention or ID | The user whose reports to unblock. |

## User management

### `/alt add`

Adds an alt account to a user's record.

| Parameter | Required | Type               | Description                        |
|:----------|:---------|:-------------------|:-----------------------------------|
| user      | ✅ Yes    | User mention or ID | The user whose alt to add.         |
| alt       | ✅ Yes    | User mention or ID | The user's alt account to add.     |

### `/alt remove`

Removes an alt account from a user's record.

| Parameter | Required | Type               | Description                        |
|:----------|:---------|:-------------------|:-----------------------------------|
| user      | ✅ Yes    | User mention or ID | The user whose alt to remove.      |
| alt       | ✅ Yes    | User mention or ID | The user's alt account to remove.  |

### `/alt view`

Views a user's alt accounts.

| Parameter | Required | Type               | Description                        |
|:----------|:---------|:-------------------|:-----------------------------------|
| user      | ✅ Yes    | User mention or ID | The user whose alts to view.       |

### `/userinfo`

**Public command.** Displays information about a user.

| Parameter | Required | Type               | Description                        |
|:----------|:---------|:-------------------|:-----------------------------------|
| user      | ✅ Yes    | User mention or ID | The user whose info to view.       |

## Issuing and revoking infractions

### `/ban`

Temporarily, or permanently, bans a user from the guild. This creates an infraction on the user's record.

| Parameter | Required | Type               | Description                                                      |
|:----------|:---------|:-------------------|:-----------------------------------------------------------------|
| user      | ✅ Yes    | User mention or ID | The user to ban.                                                 |
| reason    | ❌ No     | String             | The reason for the ban.                                          |
| duration  | ❌ No     | Duration           | The duration of the ban. If not specified, the ban is permanent. |
| rule      | ❌ No     | Integer            | The rule which was broken.                                       |

### `/kick`

Kicks a member from the guild. This creates an infraction on the user's record.

| Parameter | Required | Type                 | Description                |
|:----------|:---------|:---------------------|:---------------------------|
| member    | ✅ Yes    | Member mention or ID | The member to kick.        |
| reason    | ❌ No     | String               | The reason for the kick.   |
| rule      | ❌ No     | Integer              | The rule which was broken. |

### `/mute`

Temporarily, or permanently, mutes a user. This creates an infraction on the user's record.

| Parameter | Required | Type               | Description                                                        |
|:----------|:---------|:-------------------|:-------------------------------------------------------------------|
| user      | ✅ Yes    | User mention or ID | The user to mute.                                                  |
| reason    | ❌ No     | String             | The reason for the mute.                                           |
| duration  | ❌ No     | Duration           | The duration of the mute. If not specified, the mute is permanent. |
| rule      | ❌ No     | Integer            | The rule which was broken.                                         |

### `/unban`

Unbans a previously banned user.

| Parameter | Required | Type               | Description                        |
|:----------|:---------|:-------------------|:-----------------------------------|
| user      | ✅ Yes    | User mention or ID | The user to unban.                 |
| reason    | ❌ No     | String             | The reason for the ban revocation. |

### `/unmute`

Unmutes a previously muted user.

| Parameter | Required | Type               | Description                         |
|:----------|:---------|:-------------------|:------------------------------------|
| user      | ✅ Yes    | User mention or ID | The user to unmute.                 |
| reason    | ❌ No     | String             | The reason for the mute revocation. |

### `/warn`

Issues a warning to a user. This creates an infraction on the user's record.

| Parameter | Required | Type               | Description                 |
|:----------|:---------|:-------------------|:----------------------------|
| user      | ✅ Yes    | User mention or ID | The user to warn.           |
| reason    | ✅ Yes    | String             | The reason for the warning. |
| rule      | ❌ No     | Integer            | The rule which was broken.  |

## Infraction management

### `/history`

Displays all infractions for a user.

| Parameter | Required | Type                       | Description                                               |
|:----------|:---------|:---------------------------|:----------------------------------------------------------|
| user      | ✅ Yes    | User mention or ID         | The user whose infractions to view.                       |
| after     | ❌ No     | ID, Timestamp, or TimeSpan | Returns only infractions after the specified ID or date.  |
| before    | ❌ No     | ID, Timestamp, or TimeSpan | Returns only infractions before the specified ID or date. |
| type      | ❌ No     | InfractionType             | Returns only infractions of the specified type.           |

### `/infraction clear`

Clears all infractions for a specified user.

| Parameter | Required | Type               | Description                          |
|:----------|:---------|:-------------------|:-------------------------------------|
| user      | ✅ Yes    | User mention or ID | The user whose infractions to clear. |

### `/infraction copy`

Copies all infractions from a one user to another user. **This operation does not copy infraction IDs.**

| Parameter   | Required | Type               | Description                                |
|:------------|:---------|:-------------------|:-------------------------------------------|
| source      | ✅ Yes    | User mention or ID | The user whose infractions to copy.        |
| destination | ✅ Yes    | User mention or ID | The user who will receive the infractions. |

### `/infraction delete`

Deletes an infraction. **This process does NOT revoke the ban or mute associated with it, if any exist.**

| Parameter  | Required | Type    | Description                         |
|:-----------|:---------|:--------|:------------------------------------|
| infraction | ✅ Yes    | Integer | The ID of the infraction to delete. |

### `/infraction move`

Moves all infractions from a one user to another user. **This operation maintains infraction IDs.**

| Parameter   | Required | Type               | Description                                |
|:------------|:---------|:-------------------|:-------------------------------------------|
| source      | ✅ Yes    | User mention or ID | The user whose infractions to move.        |
| destination | ✅ Yes    | User mention or ID | The user who will receive the infractions. |

### `/infraction prune`

Removes all infractions from the database for users which no longer exist. **⚠️ This process is slow, use sparingly!**

| Parameter | Required | Type | Description |
|:----------|:---------|:-----|:------------|
| -         | -        | -    | -           |

### `/infraction view`

View the details of a specific infraction.

| Parameter  | Required | Type    | Description                       |
|:-----------|:---------|:--------|:----------------------------------|
| infraction | ✅ Yes    | Integer | The ID of the infraction to view. |

### `/messagehistory`

Views the staff-sent or staff-deleted message history for a user.

| Parameter | Required | Type               | Description                                                  |
|:----------|:---------|:-------------------|:-------------------------------------------------------------|
| user      | ✅ Yes    | User mention or ID | The user whose staff-sent or staff-deleted messages to view. |

### `/selfhistory`

**Public command.** Displays all infractions for yourself.

| Parameter | Required | Type | Description |
|:----------|:---------|:-----|:------------|
| -         | -        | -    | -           |

### `/viewmessage`

Views a staff-sent or staff-deleted message by its ID.

| Parameter | Required | Type    | Description                    |
|:----------|:---------|:--------|:-------------------------------|
| id        | ✅ Yes    | Integer | The ID of the message to view. |

## Soft moderation

### `/rule`

**Public command.** Displays the rule with the specified ID.

| Parameter | Required | Type    | Description                    |
|:----------|:---------|:--------|:-------------------------------|
| rule      | ✅ Yes    | Integer | The ID of the rule to display. |

### `/message`

Sends a private message, through Hammer, to the member. Message content is specified via a following modal. This is logged, but does not create an infraction on the user's record.

| Parameter | Required | Type                 | Description            |
|:----------|:---------|:---------------------|:-----------------------|
| member    | ✅ Yes    | Member mention or ID | The member to message. |

### `/note create`

**Intended for Staff + Guru use.** Create a note on the user. The type of the note depends on the permission level of the
creator. If a Guru runs this command, a Guru note is created. If a Moderator or above runs the command, a Staff note is
created. **This operation does NOT notify the user.**

| Parameter | Required | Type               | Description                                |
|:----------|:---------|:-------------------|:-------------------------------------------|
| user      | ✅ Yes    | User mention or ID | The user with whom the note is associated. |
| content   | ✅ Yes    | String             | The content of the note.                   |

### `/note delete`

**Intended for Staff only use.** Deletes a note by its ID.

| Parameter | Required | Type    | Description                   |
|:----------|:---------|:--------|:------------------------------|
| note      | ✅ Yes    | Integer | The ID of the note to delete. |

### `/note editcontent`

**Intended for Staff only use.** Edit the content of a note.

| Parameter | Required | Type    | Description                  |
|:----------|:---------|:--------|:-----------------------------|
| note      | ✅ Yes    | Integer | The ID of the note to edit.  |
| content   | ✅ Yes    | String  | The new content of the note. |

### `/note edittype`

**Intended for Staff only use.** Edit the type of a note (to switch between Staff and Guru notes).

| Parameter | Required | Type    | Description                  |
|:----------|:---------|:--------|:-----------------------------|
| note      | ✅ Yes    | Integer | The ID of the note to edit.  |
| content   | ✅ Yes    | String  | The new content of the note. |

### `/note view`

**Intended for Staff + Guru use.** View the details of a note. If a Guru runs this command, the note is only returned if it's a
Guru note.

| Parameter | Required | Type    | Description                 |
|:----------|:---------|:--------|:----------------------------|
| note      | ✅ Yes    | Integer | The ID of the note to view. |

### `/note viewall`

**Intended for Staff + Guru use.** View all notes stored on a user. If a Guru runs this command, only Guru notes are visible.

| Parameter | Required | Type               | Description                   |
|:----------|:---------|:-------------------|:------------------------------|
| user      | ✅ Yes    | User mention or ID | The user whose notes to view. |

## Rule management

### `/rules add`

Adds a new rule to the guild.

| Parameter | Required | Type | Description |
|:----------|:---------|:-----|:------------|
| -         | -        | -    | -           |

### `/rules delete`

Deletes a rule from the guild.

| Parameter | Required | Type    | Description                   |
|:----------|:---------|:--------|:------------------------------|
| rule      | ✅ Yes    | Integer | The ID of the rule to delete. |

### `/rules display`

Sends an embed displaying the guild rules are they are currently defined to the current - or specified - channel.

| Parameter | Required | Type                  | Description                                                                                              |
|:----------|:---------|:----------------------|:---------------------------------------------------------------------------------------------------------|
| channel   | ❌ No     | Channel mention or ID | The channel to which the rules will be sent. If not specified, the embed is sent to the current channel. |

### `/rules edit`

Modifies a rule.

| Parameter | Required | Type    | Description                   |
|:----------|:---------|:--------|:------------------------------|
| rule      | ✅ Yes    | Integer | The ID of the rule to modify. |

# Ephemeral responses

Below is a table outlining all the commands and whether or not they have ephemeral responses.

| Command                 | Ephemeral Response                                |
|:------------------------|:--------------------------------------------------|
| `/ban`                  | ✅ Yes                                             |
| `/blockreports`         | ✅ Yes                                             |
| `/history`              | ❌ No                                              |
| `/infraction clear`     | ❌ No                                              |
| `/infraction copy`      | ❌ No                                              |
| `/infraction delete`    | ❌ No                                              |
| `/infraction edit`      | ❌ No                                              |
| `/infraction move`      | ❌ No                                              |
| `/infraction prune`     | ❌ No                                              |
| `/infraction view`      | ❌ No                                              |
| `/messagehistory`       | ❌ No                                              |
| `/migrate`              | ❌ No                                              |
| `/mute`                 | ✅ Yes                                             |
| `/note create`          | ✅ Yes                                             |
| `/note delete`          | ✅ Yes                                             |
| `/note editcontent`     | ✅ Yes                                             |
| `/note edittype`        | ✅ Yes                                             |
| `/note view`            | ✅ Yes                                             |
| `/note viewall`         | ✅ Yes                                             |
| `/rule`                 | ❌ No                                              |
| `/rules add`            | ⚠️ If guild isn't configured                      |
| `/rules delete`         | ⚠️ If guild isn't configured                      |
| `/rules display`        | ➖ Interaction response is, resulting embed is not |
| `/rules setbrief`       | ⚠️ If guild isn't configured                      |
| `/rules setdescription` | ⚠️ If guild isn't configured                      |
| `/selfhistory`          | ➖ Response sent as DM                             |
| `/unmute`               | ✅ Yes                                             |
| `/unban`                | ✅ Yes                                             |
| `/unblockreports`       | ✅ Yes                                             |
| `/userinfo`             | ❌ No                                              |
| `/viewmessage`          | ❌ No                                              |
| `/warn`                 | ✅ Yes                                             |
